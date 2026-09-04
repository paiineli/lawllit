using Dapper;
using Lawllit.Api.Common;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using System.Data;

namespace Lawllit.Api.Finance.Repositories;

public sealed class TransactionREP(Func<string, IDbConnection> connectionFactory) : ITransactionREP
{
    // O Dapper mapeia por nome de coluna, então um record nomeado em vez de tupla.
    private sealed record FilteredTotals(decimal Income, decimal Expenses, decimal Investments);

    private const string SelectWithCategory = """
        SELECT
            t."Id", t."Description", t."Amount", t."Type", t."Date",
            t."IsRecurring", t."CreatedAt", t."UserId", t."CategoryId",
            c."Id", c."Name", c."Type", c."UserId"
        FROM "Transactions" t, "Categories" c
        WHERE t."CategoryId" = c."Id"
        """;

    // O mesmo filtro alimenta a página, a contagem, os totais do rodapé e a exportação.
    // Montado num lugar só para os quatro nunca divergirem.
    private static (string Where, DynamicParameters Parameters) BuildFilter(Guid userId, TransactionFilterMOD filter)
    {
        var where = """
              AND t."UserId" = @UserId
            """;

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (filter.Type.HasValue)
        {
            where += """

                  AND t."Type" = @Type
                """;
            parameters.Add("Type", (int)filter.Type.Value);
        }

        if (filter.Month.HasValue)
        {
            where += """

                  AND EXTRACT(MONTH FROM t."Date") = @Month
                """;
            parameters.Add("Month", filter.Month.Value);
        }

        if (filter.Year.HasValue)
        {
            where += """

                  AND EXTRACT(YEAR FROM t."Date") = @Year
                """;
            parameters.Add("Year", filter.Year.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            where += """

                  AND t."Description" ILIKE @Search
                """;
            parameters.Add("Search", $"%{filter.Search.Trim()}%");
        }

        return (where, parameters);
    }

    public async Task<PagedResult<TransactionMOD>> GetPageAsync(Guid userId, TransactionFilterMOD filter, CancellationToken cancellationToken)
    {
        var (where, parameters) = BuildFilter(userId, filter);

        var pageSql = $"""
            {SelectWithCategory}
            {where}
            ORDER BY t."Date" DESC, t."CreatedAt" DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"""
            SELECT COUNT(1)
            FROM "Transactions" t
            WHERE 1 = 1
            {where}
            """;

        parameters.Add("Offset", filter.Pagination.Offset);
        parameters.Add("PageSize", filter.Pagination.PageSize);

        using var connection = connectionFactory(ConnectionKeys.Lawllit);

        var items = await connection.QueryAsync<TransactionMOD, CategoryMOD, TransactionMOD>(
            new CommandDefinition(pageSql, parameters, cancellationToken: cancellationToken),
            (transaction, category) => { transaction.Category = category; return transaction; },
            splitOn: "Id");

        var totalItems = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        return new PagedResult<TransactionMOD>
        {
            Items = items.AsList(),
            TotalItems = totalItems,
            CurrentPage = filter.Pagination.CurrentPage,
            PageSize = filter.Pagination.PageSize,
        };
    }

    public async Task<List<TransactionMOD>> GetAllFilteredAsync(Guid userId, TransactionFilterMOD filter, CancellationToken cancellationToken)
    {
        var (where, parameters) = BuildFilter(userId, filter);

        var sql = $"""
            {SelectWithCategory}
            {where}
            ORDER BY t."Date" ASC, t."CreatedAt" ASC
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);

        var transactions = await connection.QueryAsync<TransactionMOD, CategoryMOD, TransactionMOD>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken),
            (transaction, category) => { transaction.Category = category; return transaction; },
            splitOn: "Id");

        return transactions.AsList();
    }

    public async Task<(decimal Income, decimal Expenses, decimal Investments)> GetFilteredTotalsAsync(Guid userId, TransactionFilterMOD filter, CancellationToken cancellationToken)
    {
        var (where, parameters) = BuildFilter(userId, filter);

        var sql = $"""
            SELECT
                COALESCE(SUM(CASE WHEN t."Type" = 0 THEN t."Amount" ELSE 0 END), 0) AS Income,
                COALESCE(SUM(CASE WHEN t."Type" = 1 THEN t."Amount" ELSE 0 END), 0) AS Expenses,
                COALESCE(SUM(CASE WHEN t."Type" = 2 THEN t."Amount" ELSE 0 END), 0) AS Investments
            FROM "Transactions" t
            WHERE 1 = 1
            {where}
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);

        var totals = await connection.QueryFirstAsync<FilteredTotals>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return (totals.Income, totals.Expenses, totals.Investments);
    }

    public async Task<TransactionMOD?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var sql = $"""
            {SelectWithCategory}
              AND t."UserId" = @UserId
              AND t."Id"     = @Id
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        var transactions = await connection.QueryAsync<TransactionMOD, CategoryMOD, TransactionMOD>(
            new CommandDefinition(sql, new { UserId = userId, Id = id }, cancellationToken: cancellationToken),
            (transaction, category) => { transaction.Category = category; return transaction; },
            splitOn: "Id");

        return transactions.FirstOrDefault();
    }

    // Os intervalos são contíguos, o anterior encosta no atual, então um único range
    // no WHERE cobre os dois períodos e o índice IX_Transactions_Date continua servindo.
    // Filtrar por EXTRACT(MONTH) descartaria o índice e varreria a tabela.
    public async Task<PeriodTotalsMOD> GetPeriodTotalsAsync(
        Guid userId,
        DateTime from,
        DateTime to,
        DateTime previousFrom,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                COALESCE(SUM(CASE WHEN "Date" >= @From         AND "Date" < @To   AND "Type" = 0 THEN "Amount" ELSE 0 END), 0) AS TotalIncome,
                COALESCE(SUM(CASE WHEN "Date" >= @From         AND "Date" < @To   AND "Type" = 1 THEN "Amount" ELSE 0 END), 0) AS TotalExpenses,
                COALESCE(SUM(CASE WHEN "Date" >= @From         AND "Date" < @To   AND "Type" = 2 THEN "Amount" ELSE 0 END), 0) AS TotalInvestments,
                COALESCE(SUM(CASE WHEN "Date" >= @From         AND "Date" < @To   AND "Type" = 1 AND "IsRecurring" THEN "Amount" ELSE 0 END), 0) AS RecurringExpenses,
                COALESCE(SUM(CASE WHEN "Date" >= @PreviousFrom AND "Date" < @From AND "Type" = 0 THEN "Amount" ELSE 0 END), 0) AS PreviousIncome,
                COALESCE(SUM(CASE WHEN "Date" >= @PreviousFrom AND "Date" < @From AND "Type" = 1 THEN "Amount" ELSE 0 END), 0) AS PreviousExpenses,
                COALESCE(SUM(CASE WHEN "Date" >= @PreviousFrom AND "Date" < @From AND "Type" = 2 THEN "Amount" ELSE 0 END), 0) AS PreviousInvestments
            FROM "Transactions"
            WHERE "UserId" = @UserId
              AND "Date"  >= @PreviousFrom
              AND "Date"   < @To
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);

        return await connection.QueryFirstAsync<PeriodTotalsMOD>(new CommandDefinition(
            sql,
            new { UserId = userId, From = from, To = to, PreviousFrom = previousFrom },
            cancellationToken: cancellationToken));
    }

    // Traz o gasto do período e a média histórica da mesma categoria de uma vez. O HAVING
    // descarta categoria que só teve gasto no histórico, para não virar fatia zerada no gráfico.
    public async Task<List<CategorySpendMOD>> GetCategorySpendAsync(
        Guid userId,
        DateTime from,
        DateTime to,
        DateTime referenceFrom,
        int referenceMonths,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c."Name" AS CategoryName,
                COALESCE(SUM(CASE WHEN t."Date" >= @From THEN t."Amount" ELSE 0 END), 0) AS Total,
                COALESCE(
                    SUM(CASE WHEN t."Date" < @From THEN t."Amount" ELSE 0 END)
                        / NULLIF(@ReferenceMonths, 0),
                    0) AS ReferenceAverage
            FROM "Transactions" t, "Categories" c
            WHERE t."CategoryId" = c."Id"
              AND t."UserId"     = @UserId
              AND t."Type"       = 1
              AND t."Date"      >= @ReferenceFrom
              AND t."Date"       < @To
            GROUP BY c."Name"
            HAVING COALESCE(SUM(CASE WHEN t."Date" >= @From THEN t."Amount" ELSE 0 END), 0) > 0
            ORDER BY Total DESC
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);

        var rows = await connection.QueryAsync<CategorySpendMOD>(new CommandDefinition(
            sql,
            new { UserId = userId, From = from, To = to, ReferenceFrom = referenceFrom, ReferenceMonths = referenceMonths },
            cancellationToken: cancellationToken));

        return rows.AsList();
    }

    // Única consulta do projeto sem recorte de período. Responde quanto já foi aportado
    // até o fim do período em tela, e em quantos meses distintos isso aconteceu.
    public async Task<InvestedTotalMOD> GetInvestedTotalAsync(Guid userId, DateTime upTo, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                COALESCE(SUM("Amount"), 0)                             AS Total,
                COUNT(DISTINCT DATE_TRUNC('month', "Date"))::int       AS MonthCount
            FROM "Transactions"
            WHERE "UserId" = @UserId
              AND "Type"   = 2
              AND "Date"   < @UpTo
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);

        return await connection.QueryFirstAsync<InvestedTotalMOD>(new CommandDefinition(
            sql,
            new { UserId = userId, UpTo = upTo },
            cancellationToken: cancellationToken));
    }

    public async Task<decimal> GetUpcomingExpensesAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var today = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

        const string sql = """
            SELECT COALESCE(SUM("Amount"), 0)
            FROM "Transactions"
            WHERE "UserId" = @UserId
              AND "Type"   = 1
              AND "Date"  >= @From
              AND "Date"   < @To
              AND "Date"   > @Today
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        return await connection.ExecuteScalarAsync<decimal>(new CommandDefinition(
            sql,
            new { UserId = userId, From = from, To = to, Today = today },
            cancellationToken: cancellationToken));
    }

    public async Task<int> GetPendingRecurringCountAsync(Guid userId, int month, int year, CancellationToken cancellationToken)
    {
        const string recurringCountSql = """
            SELECT COUNT(1)
            FROM "Transactions"
            WHERE "UserId"                   = @UserId
              AND "IsRecurring"              = TRUE
              AND EXTRACT(MONTH FROM "Date") = @Month
              AND EXTRACT(YEAR  FROM "Date") = @Year
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);

        var currentMonthRecurring = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            recurringCountSql,
            new { UserId = userId, Month = month, Year = year },
            cancellationToken: cancellationToken));

        // Já existe recorrente lançado no mês, então não há nada pendente para importar.
        if (currentMonthRecurring > 0) return 0;

        var previousMonth = new DateTime(year, month, 1).AddMonths(-1);

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            recurringCountSql,
            new { UserId = userId, previousMonth.Month, previousMonth.Year },
            cancellationToken: cancellationToken));
    }

    public async Task<List<TransactionMOD>> GetRecurringForImportAsync(Guid userId, int month, int year, CancellationToken cancellationToken)
    {
        var previousMonth = new DateTime(year, month, 1).AddMonths(-1);

        var sql = $"""
            {SelectWithCategory}
              AND t."UserId"                   = @UserId
              AND t."IsRecurring"              = TRUE
              AND EXTRACT(MONTH FROM t."Date") = @Month
              AND EXTRACT(YEAR  FROM t."Date") = @Year
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        var transactions = await connection.QueryAsync<TransactionMOD, CategoryMOD, TransactionMOD>(
            new CommandDefinition(
                sql,
                new { UserId = userId, previousMonth.Month, previousMonth.Year },
                cancellationToken: cancellationToken),
            (transaction, category) => { transaction.Category = category; return transaction; },
            splitOn: "Id");

        return transactions.AsList();
    }

    public async Task<List<MonthlyTrendMOD>> GetMonthlyTrendAsync(Guid userId, int toMonth, int toYear, int monthCount, CancellationToken cancellationToken)
    {
        var months = new List<(int Month, int Year)>();
        var currentMonth = toMonth;
        var currentYear = toYear;

        for (var index = 0; index < monthCount; index++)
        {
            months.Add((currentMonth, currentYear));
            if (--currentMonth == 0) { currentMonth = 12; currentYear--; }
        }

        months.Reverse();

        var startDate = new DateTime(months[0].Year, months[0].Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(toYear, toMonth, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);

        const string sql = """
            SELECT
                EXTRACT(MONTH FROM "Date")::int AS Month,
                EXTRACT(YEAR  FROM "Date")::int AS Year,
                COALESCE(SUM(CASE WHEN "Type" = 0 THEN "Amount" ELSE 0 END), 0) AS Income,
                COALESCE(SUM(CASE WHEN "Type" = 1 THEN "Amount" ELSE 0 END), 0) AS Expenses,
                COALESCE(SUM(CASE WHEN "Type" = 2 THEN "Amount" ELSE 0 END), 0) AS Investments
            FROM "Transactions"
            WHERE "UserId" = @UserId
              AND "Date"  >= @StartDate
              AND "Date"   < @EndDate
            GROUP BY EXTRACT(YEAR FROM "Date"), EXTRACT(MONTH FROM "Date")
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        var rows = await connection.QueryAsync<MonthlyTrendMOD>(new CommandDefinition(
            sql,
            new { UserId = userId, StartDate = startDate, EndDate = endDate },
            cancellationToken: cancellationToken));

        var rowsByMonth = rows.ToDictionary(row => (row.Month, row.Year));

        // Mês sem lançamento não volta do banco, mas o gráfico precisa da série completa.
        return months
            .Select(month => rowsByMonth.TryGetValue((month.Month, month.Year), out var trend)
                ? trend
                : new MonthlyTrendMOD(month.Month, month.Year, 0, 0, 0))
            .ToList();
    }

    public async Task AddAsync(TransactionMOD transaction, CancellationToken cancellationToken)
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

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                transaction.Id,
                transaction.Description,
                transaction.Amount,
                Type = (int)transaction.Type,
                transaction.Date,
                transaction.IsRecurring,
                transaction.CreatedAt,
                transaction.UserId,
                transaction.CategoryId,
            },
            cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(TransactionMOD transaction, CancellationToken cancellationToken)
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

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                transaction.Description,
                transaction.Amount,
                Type = (int)transaction.Type,
                transaction.Date,
                transaction.IsRecurring,
                transaction.CategoryId,
                transaction.Id,
                transaction.UserId,
            },
            cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM "Transactions"
            WHERE "Id"     = @TransactionId
              AND "UserId" = @UserId
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { UserId = userId, TransactionId = transactionId },
            cancellationToken: cancellationToken));
    }
}

#region Interfaces

public interface ITransactionREP
{
    Task<PagedResult<TransactionMOD>> GetPageAsync(Guid userId, TransactionFilterMOD filter, CancellationToken cancellationToken);
    Task<List<TransactionMOD>> GetAllFilteredAsync(Guid userId, TransactionFilterMOD filter, CancellationToken cancellationToken);
    Task<(decimal Income, decimal Expenses, decimal Investments)> GetFilteredTotalsAsync(Guid userId, TransactionFilterMOD filter, CancellationToken cancellationToken);
    Task<TransactionMOD?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<PeriodTotalsMOD> GetPeriodTotalsAsync(Guid userId, DateTime from, DateTime to, DateTime previousFrom, CancellationToken cancellationToken);
    Task<List<CategorySpendMOD>> GetCategorySpendAsync(Guid userId, DateTime from, DateTime to, DateTime referenceFrom, int referenceMonths, CancellationToken cancellationToken);
    Task<InvestedTotalMOD> GetInvestedTotalAsync(Guid userId, DateTime upTo, CancellationToken cancellationToken);
    Task<decimal> GetUpcomingExpensesAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken);
    Task<int> GetPendingRecurringCountAsync(Guid userId, int month, int year, CancellationToken cancellationToken);
    Task<List<TransactionMOD>> GetRecurringForImportAsync(Guid userId, int month, int year, CancellationToken cancellationToken);
    Task<List<MonthlyTrendMOD>> GetMonthlyTrendAsync(Guid userId, int toMonth, int toYear, int monthCount, CancellationToken cancellationToken);
    Task AddAsync(TransactionMOD transaction, CancellationToken cancellationToken);
    Task UpdateAsync(TransactionMOD transaction, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken);
}

#endregion
