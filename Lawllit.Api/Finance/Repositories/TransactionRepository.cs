using Dapper;
using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Models.Finance;
using Npgsql;
using System.Text;

namespace Lawllit.Api.Finance.Repositories;

public class TransactionRepository(string connectionString) : ITransactionRepository
{
    private NpgsqlConnection CreateConnection() => new(connectionString);

    private sealed record MonthTotals(decimal TotalIncome, decimal TotalExpenses);

    public async Task<List<Transaction>> GetFilteredAsync(Guid userId, string? type, int? month, int? year, string? search = null)
    {
        var sql = new StringBuilder("""
            SELECT
                t."Id", t."Description", t."Amount", t."Type", t."Date",
                t."IsRecurring", t."CreatedAt", t."UserId", t."CategoryId",
                c."Id", c."Name", c."Type", c."UserId"
            FROM "Transactions" t
            INNER JOIN "Categories" c ON t."CategoryId" = c."Id"
            WHERE t."UserId" = @UserId
            """);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (Enum.TryParse<TransactionType>(type, ignoreCase: true, out var parsedType))
        {
            sql.Append(""" AND t."Type" = @Type""");
            parameters.Add("Type", (int)parsedType);
        }

        if (month.HasValue)
        {
            sql.Append(""" AND EXTRACT(MONTH FROM t."Date") = @Month""");
            parameters.Add("Month", month.Value);
        }

        if (year.HasValue)
        {
            sql.Append(""" AND EXTRACT(YEAR FROM t."Date") = @Year""");
            parameters.Add("Year", year.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql.Append(""" AND t."Description" ILIKE @Search""");
            parameters.Add("Search", $"%{search}%");
        }

        sql.Append(""" ORDER BY t."Date" DESC""");

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<Transaction, Category, Transaction>(
            sql.ToString(),
            (transaction, category) => { transaction.Category = category; return transaction; },
            parameters,
            splitOn: "Id");

        return result.ToList();
    }

    public async Task<Transaction?> GetByIdAsync(Guid userId, Guid id)
    {
        const string sql = """
            SELECT
                t."Id", t."Description", t."Amount", t."Type", t."Date",
                t."IsRecurring", t."CreatedAt", t."UserId", t."CategoryId",
                c."Id", c."Name", c."Type", c."UserId"
            FROM "Transactions" t
            INNER JOIN "Categories" c ON t."CategoryId" = c."Id"
            WHERE t."UserId" = @UserId
              AND t."Id"     = @Id
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<Transaction, Category, Transaction>(
            sql,
            (transaction, category) => { transaction.Category = category; return transaction; },
            new { UserId = userId, Id = id },
            splitOn: "Id");

        return result.FirstOrDefault();
    }

    public async Task<MonthlySummary> GetSummaryAsync(Guid userId, int month, int year)
    {
        const string totalsSql = """
            SELECT
                COALESCE(SUM(CASE WHEN "Type" = 0 THEN "Amount" ELSE 0 END), 0) AS TotalIncome,
                COALESCE(SUM(CASE WHEN "Type" = 1 THEN "Amount" ELSE 0 END), 0) AS TotalExpenses
            FROM "Transactions"
            WHERE "UserId"                          = @UserId
              AND EXTRACT(MONTH FROM "Date")        = @Month
              AND EXTRACT(YEAR  FROM "Date")        = @Year
            """;

        const string categoryBreakdownSql = """
            SELECT c."Name" AS CategoryName, SUM(t."Amount") AS Total
            FROM "Transactions" t
            INNER JOIN "Categories" c ON t."CategoryId" = c."Id"
            WHERE t."UserId"                         = @UserId
              AND t."Type"                           = 1
              AND EXTRACT(MONTH FROM t."Date")       = @Month
              AND EXTRACT(YEAR  FROM t."Date")       = @Year
            GROUP BY c."Name"
            ORDER BY Total DESC
            """;

        var queryParams = new { UserId = userId, Month = month, Year = year };

        using var connection = CreateConnection();
        var totals = await connection.QueryFirstAsync<MonthTotals>(totalsSql, queryParams);
        var byCategory = (await connection.QueryAsync<CategorySummary>(categoryBreakdownSql, queryParams)).ToList();

        return new MonthlySummary(month, year, totals.TotalIncome, totals.TotalExpenses, totals.TotalIncome - totals.TotalExpenses, byCategory);
    }

    public async Task<decimal> GetUpcomingExpensesAsync(Guid userId, int month, int year)
    {
        var today = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

        const string sql = """
            SELECT COALESCE(SUM("Amount"), 0)
            FROM "Transactions"
            WHERE "UserId"                         = @UserId
              AND "Type"                           = 1
              AND EXTRACT(MONTH FROM "Date")       = @Month
              AND EXTRACT(YEAR  FROM "Date")       = @Year
              AND "Date" > @Today
            """;

        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(sql, new { UserId = userId, Month = month, Year = year, Today = today });
    }

    public async Task<int> GetPendingRecurringCountAsync(Guid userId, int month, int year)
    {
        const string hasCurrentSql = """
            SELECT COUNT(1)
            FROM "Transactions"
            WHERE "UserId"                         = @UserId
              AND "IsRecurring"                    = TRUE
              AND EXTRACT(MONTH FROM "Date")       = @Month
              AND EXTRACT(YEAR  FROM "Date")       = @Year
            """;

        using var connection = CreateConnection();
        var hasCurrentRecurring = await connection.ExecuteScalarAsync<int>(hasCurrentSql, new { UserId = userId, Month = month, Year = year }) > 0;

        if (hasCurrentRecurring) return 0;

        var previousMonthDate = new DateTime(year, month, 1).AddMonths(-1);

        const string previousCountSql = """
            SELECT COUNT(1)
            FROM "Transactions"
            WHERE "UserId"                         = @UserId
              AND "IsRecurring"                    = TRUE
              AND EXTRACT(MONTH FROM "Date")       = @Month
              AND EXTRACT(YEAR  FROM "Date")       = @Year
            """;

        return await connection.ExecuteScalarAsync<int>(previousCountSql, new { UserId = userId, Month = previousMonthDate.Month, Year = previousMonthDate.Year });
    }

    public async Task<List<Transaction>> GetRecurringForImportAsync(Guid userId, int month, int year)
    {
        var previousMonthDate = new DateTime(year, month, 1).AddMonths(-1);

        const string sql = """
            SELECT
                t."Id", t."Description", t."Amount", t."Type", t."Date",
                t."IsRecurring", t."CreatedAt", t."UserId", t."CategoryId",
                c."Id", c."Name", c."Type", c."UserId"
            FROM "Transactions" t
            INNER JOIN "Categories" c ON t."CategoryId" = c."Id"
            WHERE t."UserId"                         = @UserId
              AND t."IsRecurring"                    = TRUE
              AND EXTRACT(MONTH FROM t."Date")       = @Month
              AND EXTRACT(YEAR  FROM t."Date")       = @Year
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<Transaction, Category, Transaction>(
            sql,
            (transaction, category) => { transaction.Category = category; return transaction; },
            new { UserId = userId, Month = previousMonthDate.Month, Year = previousMonthDate.Year },
            splitOn: "Id");

        return result.ToList();
    }

    public async Task<List<MonthlyTrend>> GetMonthlyTrendAsync(Guid userId, int toMonth, int toYear, int count = 6)
    {
        var months = new List<(int Month, int Year)>();
        var currentMonth = toMonth;
        var currentYear = toYear;

        for (var i = 0; i < count; i++)
        {
            months.Add((currentMonth, currentYear));
            if (--currentMonth == 0) { currentMonth = 12; currentYear--; }
        }

        months.Reverse();

        var startDate = new DateTime(months[0].Year, months[0].Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(toYear, toMonth, DateTime.DaysInMonth(toYear, toMonth), 23, 59, 59, DateTimeKind.Utc);

        const string sql = """
            SELECT
                EXTRACT(MONTH FROM "Date")::int AS Month,
                EXTRACT(YEAR  FROM "Date")::int AS Year,
                COALESCE(SUM(CASE WHEN "Type" = 0 THEN "Amount" ELSE 0 END), 0) AS Income,
                COALESCE(SUM(CASE WHEN "Type" = 1 THEN "Amount" ELSE 0 END), 0) AS Expenses
            FROM "Transactions"
            WHERE "UserId"   = @UserId
              AND "Date"    >= @StartDate
              AND "Date"    <= @EndDate
            GROUP BY EXTRACT(YEAR FROM "Date"), EXTRACT(MONTH FROM "Date")
            """;

        using var connection = CreateConnection();
        var rawData = (await connection.QueryAsync<MonthlyTrend>(sql, new { UserId = userId, StartDate = startDate, EndDate = endDate }))
            .ToDictionary(row => (row.Month, row.Year));

        return months
            .Select(m => rawData.TryGetValue((m.Month, m.Year), out var trend)
                ? trend
                : new MonthlyTrend(m.Month, m.Year, 0, 0))
            .ToList();
    }

    public async Task AddAsync(Transaction transaction)
    {
        const string sql = """
            INSERT INTO "Transactions" (
                "Id", "Description", "Amount", "Type", "Date",
                "IsRecurring", "CreatedAt", "UserId", "CategoryId"
            ) VALUES (
                @Id, @Description, @Amount, @Type, @Date,
                @IsRecurring, @CreatedAt, @UserId, @CategoryId
            )
            """;

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            transaction.Id,
            transaction.Description,
            transaction.Amount,
            Type = (int)transaction.Type,
            transaction.Date,
            transaction.IsRecurring,
            transaction.CreatedAt,
            transaction.UserId,
            transaction.CategoryId
        });
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        const string sql = """
            UPDATE "Transactions" SET
                "Description" = @Description,
                "Amount"      = @Amount,
                "Type"        = @Type,
                "Date"        = @Date,
                "IsRecurring" = @IsRecurring,
                "CategoryId"  = @CategoryId
            WHERE "Id"     = @Id
              AND "UserId" = @UserId
            """;

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            transaction.Description,
            transaction.Amount,
            Type = (int)transaction.Type,
            transaction.Date,
            transaction.IsRecurring,
            transaction.CategoryId,
            transaction.Id,
            transaction.UserId
        });
    }

    public async Task DeleteAsync(Guid transactionId)
    {
        const string sql = """
            DELETE FROM "Transactions"
            WHERE "Id" = @TransactionId
            """;

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new { TransactionId = transactionId });
    }
}
