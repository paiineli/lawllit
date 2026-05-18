using Lawllit.Models.Finance;

namespace Lawllit.Api.Finance.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllByUserAsync(Guid userId);
    Task<Category?> GetByIdAsync(Guid userId, Guid id);
    Task<bool> ExistsAsync(Guid userId, string name, TransactionType type, Guid? excludeId = null);
    Task<bool> HasTransactionsAsync(Guid userId, Guid categoryId);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Guid categoryId);
}
