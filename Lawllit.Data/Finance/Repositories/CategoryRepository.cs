using Lawllit.Data.Finance.Entities;
using Lawllit.Data.Finance.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lawllit.Data.Finance.Repositories;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<List<Category>> GetAllByUserAsync(Guid userId) =>
        db.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public Task<Category?> GetByIdAsync(Guid userId, Guid id) =>
        db.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == id);

    public Task<bool> ExistsAsync(Guid userId, string name, TransactionType type, Guid? excludeId = null) =>
        db.Categories.AnyAsync(c =>
            c.UserId == userId &&
            c.Type == type &&
            c.Name.ToLower() == name.ToLower() &&
            (excludeId == null || c.Id != excludeId));

    public Task<bool> HasTransactionsAsync(Guid userId, Guid categoryId) =>
        db.Transactions.AnyAsync(t => t.UserId == userId && t.CategoryId == categoryId);

    public async Task AddAsync(Category category) => await db.Categories.AddAsync(category);

    public Task DeleteAsync(Category category)
    {
        db.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
