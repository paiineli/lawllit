using Lawllit.Models.Finance;

namespace Lawllit.Api.Finance.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<User?> GetByConfirmationTokenAsync(string token);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid userId);
}
