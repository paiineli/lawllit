using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionListViewModel> GetListViewModelAsync(Guid userId, string? type, int? month, int? year, string? search);
    Task<Result> CreateAsync(Guid userId, TransactionFormViewModel form);
    Task<Result> EditAsync(Guid userId, Guid id, TransactionFormViewModel form);
    Task<Result> DeleteAsync(Guid userId, Guid id);
    Task<int> ImportRecurringTransactionsAsync(Guid userId, int month, int year);
}
