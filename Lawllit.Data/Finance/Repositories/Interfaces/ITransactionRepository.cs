using Lawllit.Data.Finance.Entities;

namespace Lawllit.Data.Finance.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetFilteredAsync(Guid userId, string? type, int? month, int? year, string? search = null);
    Task<Transaction?> GetByIdAsync(Guid userId, Guid id);
    Task<MonthlySummary> GetSummaryAsync(Guid userId, int month, int year);
    Task<decimal> GetUpcomingExpensesAsync(Guid userId, int month, int year);
    Task<List<MonthlyTrend>> GetMonthlyTrendAsync(Guid userId, int toMonth, int toYear, int count = 6);
    Task<int> GetPendingRecurringCountAsync(Guid userId, int month, int year);
    Task<List<Transaction>> GetRecurringForImportAsync(Guid userId, int month, int year);
    Task AddAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);
    Task SaveChangesAsync();
}
