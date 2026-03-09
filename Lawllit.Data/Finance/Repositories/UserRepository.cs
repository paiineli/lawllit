using Lawllit.Data.Finance.Entities;
using Lawllit.Data.Finance.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lawllit.Data.Finance.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByGoogleIdAsync(string googleId) =>
        db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

    public Task<User?> GetByConfirmationTokenAsync(string token) =>
        db.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);

    public Task<User?> GetByPasswordResetTokenAsync(string token) =>
        db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);

    public async Task AddAsync(User user) => await db.Users.AddAsync(user);

    public Task DeleteAsync(User user)
    {
        db.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
