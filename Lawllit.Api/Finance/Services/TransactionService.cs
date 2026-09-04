using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;

namespace Lawllit.Api.Finance.Services;

public sealed class TransactionService(
    ITransactionREP transactionRepository,
    ICategoryREP categoryRepository) : ITransactionService
{
    public async Task<TransactionPageMOD> GetPageAsync(Guid userId, TransactionFilterMOD filter, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        filter.Month ??= now.Month;
        filter.Year ??= now.Year;

        var page = await transactionRepository.GetPageAsync(userId, filter, cancellationToken);
        var totals = await transactionRepository.GetFilteredTotalsAsync(userId, filter, cancellationToken);
        var pendingRecurringCount = await transactionRepository.GetPendingRecurringCountAsync(userId, filter.Month.Value, filter.Year.Value, cancellationToken);

        return new TransactionPageMOD
        {
            Page = page,
            Month = filter.Month.Value,
            Year = filter.Year.Value,
            TotalIncome = totals.Income,
            TotalExpenses = totals.Expenses,
            TotalInvestments = totals.Investments,
            PendingRecurringCount = pendingRecurringCount,
        };
    }

    public Task<TransactionMOD?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        => transactionRepository.GetByIdAsync(userId, id, cancellationToken);

    public async Task<Result> CreateAsync(Guid userId, TransactionSaveMOD transaction, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(userId, transaction.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure("Msg_CategoryNotFound");

        await transactionRepository.AddAsync(new TransactionMOD
        {
            Id = Guid.NewGuid(),
            Description = transaction.Description?.Trim() ?? string.Empty,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Date = DateTime.SpecifyKind(transaction.Date.Date, DateTimeKind.Utc),
            UserId = userId,
            CategoryId = transaction.CategoryId,
            IsRecurring = transaction.IsRecurring,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> EditAsync(Guid userId, TransactionSaveMOD transaction, CancellationToken cancellationToken)
    {
        var existing = await transactionRepository.GetByIdAsync(userId, transaction.Id, cancellationToken);
        if (existing is null)
            return Result.Failure("Msg_TransNotFound");

        var category = await categoryRepository.GetByIdAsync(userId, transaction.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure("Msg_CategoryNotFound");

        existing.Description = transaction.Description?.Trim() ?? string.Empty;
        existing.Amount = transaction.Amount;
        existing.Type = transaction.Type;
        existing.Date = DateTime.SpecifyKind(transaction.Date.Date, DateTimeKind.Utc);
        existing.CategoryId = transaction.CategoryId;
        existing.IsRecurring = transaction.IsRecurring;

        await transactionRepository.UpdateAsync(existing, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(userId, id, cancellationToken);
        if (transaction is null)
            return Result.Failure("Msg_TransNotFound");

        await transactionRepository.DeleteAsync(userId, transaction.Id, cancellationToken);
        return Result.Success();
    }

    public async Task<ImportRecurringResultMOD> ImportRecurringAsync(Guid userId, ImportRecurringMOD import, CancellationToken cancellationToken)
    {
        var previousTransactions = await transactionRepository.GetRecurringForImportAsync(userId, import.Month, import.Year, cancellationToken);

        foreach (var previousTransaction in previousTransactions)
        {
            // Dia 31 virando fevereiro cai no último dia válido do mês de destino.
            var dayOfMonth = Math.Min(previousTransaction.Date.Day, DateTime.DaysInMonth(import.Year, import.Month));

            await transactionRepository.AddAsync(new TransactionMOD
            {
                Id = Guid.NewGuid(),
                Description = previousTransaction.Description,
                Amount = previousTransaction.Amount,
                Type = previousTransaction.Type,
                Date = DateTime.SpecifyKind(new DateTime(import.Year, import.Month, dayOfMonth), DateTimeKind.Utc),
                UserId = userId,
                CategoryId = previousTransaction.CategoryId,
                IsRecurring = true,
                CreatedAt = DateTime.UtcNow,
            }, cancellationToken);
        }

        return new ImportRecurringResultMOD { ImportedCount = previousTransactions.Count };
    }
}

#region Interfaces

public interface ITransactionService
{
    Task<TransactionPageMOD> GetPageAsync(Guid userId, TransactionFilterMOD filter, CancellationToken cancellationToken);
    Task<TransactionMOD?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<Result> CreateAsync(Guid userId, TransactionSaveMOD transaction, CancellationToken cancellationToken);
    Task<Result> EditAsync(Guid userId, TransactionSaveMOD transaction, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<ImportRecurringResultMOD> ImportRecurringAsync(Guid userId, ImportRecurringMOD import, CancellationToken cancellationToken);
}

#endregion
