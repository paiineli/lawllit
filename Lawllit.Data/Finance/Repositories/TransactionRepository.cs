using Lawllit.Data.Finance.Entities;
using Lawllit.Data.Finance.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lawllit.Data.Finance.Repositories;

public class TransactionRepository(AppDbContext db) : ITransactionRepository
{
    public Task<List<Transaction>> GetFilteredAsync(Guid userId, string? type, int? month, int? year, string? search = null)
    {
        var query = db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (Enum.TryParse<TransactionType>(type, ignoreCase: true, out var parsedType))
            query = query.Where(t => t.Type == parsedType);

        if (month.HasValue) query = query.Where(t => t.Date.Month == month.Value);
        if (year.HasValue) query = query.Where(t => t.Date.Year == year.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => EF.Functions.ILike(t.Description, $"%{search}%"));

        return query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public Task<Transaction?> GetByIdAsync(Guid userId, Guid id) =>
        db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == id);

    public async Task<MonthlySummary> GetSummaryAsync(Guid userId, int month, int year)
    {
        var transactions = await db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date.Month == month && t.Date.Year == year)
            .ToListAsync();

        var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var byCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategorySummary(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(c => c.Total)
            .ToList();

        return new MonthlySummary(month, year, income, expenses, income - expenses, byCategory);
    }

    public async Task<decimal> GetUpcomingExpensesAsync(Guid userId, int month, int year)
    {
        var today = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
        return await db.Transactions
            .Where(t => t.UserId == userId
                     && t.Type == TransactionType.Expense
                     && t.Date.Month == month
                     && t.Date.Year == year
                     && t.Date > today)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;
    }

    public async Task<int> GetPendingRecurringCountAsync(Guid userId, int month, int year)
    {
        var hasCurrentRecurring = await db.Transactions
            .AnyAsync(t => t.UserId == userId && t.IsRecurring
                        && t.Date.Month == month && t.Date.Year == year);

        if (hasCurrentRecurring) return 0;

        var previousMonthDate = new DateTime(year, month, 1).AddMonths(-1);
        return await db.Transactions
            .CountAsync(t => t.UserId == userId && t.IsRecurring
                          && t.Date.Month == previousMonthDate.Month && t.Date.Year == previousMonthDate.Year);
    }

    public Task<List<Transaction>> GetRecurringForImportAsync(Guid userId, int month, int year)
    {
        var previousMonthDate = new DateTime(year, month, 1).AddMonths(-1);
        return db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.IsRecurring
                     && t.Date.Month == previousMonthDate.Month && t.Date.Year == previousMonthDate.Year)
            .ToListAsync();
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

        var allTransactions = await db.Transactions
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        return months.Select(monthEntry => new MonthlyTrend(
            monthEntry.Month, monthEntry.Year,
            allTransactions.Where(t => t.Date.Month == monthEntry.Month && t.Date.Year == monthEntry.Year && t.Type == TransactionType.Income).Sum(t => t.Amount),
            allTransactions.Where(t => t.Date.Month == monthEntry.Month && t.Date.Year == monthEntry.Year && t.Type == TransactionType.Expense).Sum(t => t.Amount)
        )).ToList();
    }

    public async Task AddAsync(Transaction transaction) => await db.Transactions.AddAsync(transaction);

    public Task DeleteAsync(Transaction transaction)
    {
        db.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
