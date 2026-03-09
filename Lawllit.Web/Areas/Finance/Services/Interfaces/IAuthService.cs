using Lawllit.Data.Finance.Entities;

namespace Lawllit.Web.Areas.Finance.Services.Interfaces;

public interface IAuthService
{
    Task<Result<User>> ValidateLoginAsync(string email, string password);
    Task<Result<User>> RegisterAsync(string name, string email, string password, string language);
    Task<Result<User>> ConfirmEmailAsync(string token);
    Task<bool> ValidatePasswordResetTokenAsync(string token);
    Task<Result<(User User, string Token)>> BeginPasswordResetAsync(string email);
    Task<Result> ResetPasswordAsync(string token, string newPassword);
    Task<User> GetOrCreateGoogleUserAsync(string googleId, string email, string name);
}
