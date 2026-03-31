using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance;
using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services;

public class TransactionService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository) : ITransactionService
{
    public async Task<TransactionListViewModel> GetListViewModelAsync(Guid userId, string? type, int? month, int? year, string? search)
    {
        var now = DateTime.Now;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;

        var transactions = await transactionRepository.GetFilteredAsync(userId, type, selectedMonth, selectedYear, search);
        var categories = await categoryRepository.GetAllByUserAsync(userId);
        var pendingRecurringCount = await transactionRepository.GetPendingRecurringCountAsync(userId, selectedMonth, selectedYear);

        return new TransactionListViewModel
        {
            Transactions = transactions,
            Categories = categories,
            FilterType = type,
            FilterSearch = search,
            FilterMonth = selectedMonth,
            FilterYear = selectedYear,
            TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
            PendingRecurringCount = pendingRecurringCount,
        };
    }

    public async Task<Result> CreateAsync(Guid userId, TransactionFormViewModel form)
    {
        var category = await categoryRepository.GetByIdAsync(userId, form.CategoryId);
        if (category is null)
            return Result.Failure("Msg_CategoryNotFound");

        await transactionRepository.AddAsync(new Transaction
        {
            Id = Guid.NewGuid(),
            Description = form.Description?.Trim() ?? "",
            Amount = form.Amount,
            Type = form.Type,
            Date = DateTime.SpecifyKind(form.Date.Date, DateTimeKind.Utc),
            UserId = userId,
            CategoryId = form.CategoryId,
            IsRecurring = form.IsRecurring,
            CreatedAt = DateTime.UtcNow,
        });

        return Result.Success();
    }

    public async Task<Result> EditAsync(Guid userId, Guid id, TransactionFormViewModel form)
    {
        var transaction = await transactionRepository.GetByIdAsync(userId, id);
        if (transaction is null)
            return Result.Failure("Msg_TransNotFound");

        var category = await categoryRepository.GetByIdAsync(userId, form.CategoryId);
        if (category is null)
            return Result.Failure("Msg_CategoryNotFound");

        transaction.Description = form.Description?.Trim() ?? "";
        transaction.Amount = form.Amount;
        transaction.Type = form.Type;
        transaction.Date = DateTime.SpecifyKind(form.Date.Date, DateTimeKind.Utc);
        transaction.CategoryId = form.CategoryId;
        transaction.IsRecurring = form.IsRecurring;

        await transactionRepository.UpdateAsync(transaction);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id)
    {
        var transaction = await transactionRepository.GetByIdAsync(userId, id);
        if (transaction is null)
            return Result.Failure("Msg_TransNotFound");

        await transactionRepository.DeleteAsync(transaction.Id);
        return Result.Success();
    }

    public async Task<int> ImportRecurringTransactionsAsync(Guid userId, int month, int year)
    {
        var previousTransactions = await transactionRepository.GetRecurringForImportAsync(userId, month, year);

        foreach (var previousTransaction in previousTransactions)
        {
            var dayOfMonth = Math.Min(previousTransaction.Date.Day, DateTime.DaysInMonth(year, month));
            var newDate = DateTime.SpecifyKind(new DateTime(year, month, dayOfMonth), DateTimeKind.Utc);

            await transactionRepository.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                Description = previousTransaction.Description,
                Amount = previousTransaction.Amount,
                Type = previousTransaction.Type,
                Date = newDate,
                UserId = userId,
                CategoryId = previousTransaction.CategoryId,
                IsRecurring = true,
                CreatedAt = DateTime.UtcNow,
            });
        }

        return previousTransactions.Count;
    }
}
