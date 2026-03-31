using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance;

namespace Lawllit.Api.Finance.Services;

public class AuthService(IUserRepository userRepository) : IAuthService
{
    public async Task<Result<User>> ValidateLoginAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user is null || user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Result<User>.Failure("Msg_WrongCredentials");
        if (!user.EmailConfirmed)
            return Result<User>.Failure("Msg_EmailNotConfirmed");
        return Result<User>.Success(user);
    }

    public async Task<Result<User>> RegisterAsync(string name, string email, string password, string language)
    {
        var existingUser = await userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
            return Result<User>.Failure("Msg_EmailInUse");

        var confirmToken = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = StringHelpers.ToTitleCase(name),
            Email = email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            EmailConfirmed = false,
            EmailConfirmationToken = confirmToken,
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            Language = language,
            Currency = language == "en-US" ? "USD" : "BRL",
        };

        await userRepository.AddAsync(user);
        return Result<User>.Success(user);
    }

    public async Task<Result<User>> ConfirmEmailAsync(string token)
    {
        var user = await userRepository.GetByConfirmationTokenAsync(token);
        if (user is null || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            return Result<User>.Failure("Msg_InvalidLink");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;
        await userRepository.UpdateAsync(user);
        return Result<User>.Success(user);
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string token)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(token);
        return user is not null && user.PasswordResetTokenExpiry >= DateTime.UtcNow;
    }

    public async Task<Result<(User User, string Token)>> BeginPasswordResetAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user is null || !user.EmailConfirmed)
            return Result<(User, string)>.Failure("Msg_NotFound");

        var resetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await userRepository.UpdateAsync(user);
        return Result<(User, string)>.Success((user, resetToken));
    }

    public async Task<Result> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(token);
        if (user is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return Result.Failure("Msg_InvalidLink");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await userRepository.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<User> GetOrCreateGoogleUserAsync(string googleId, string email, string name, string language)
    {
        var user = await userRepository.GetByGoogleIdAsync(googleId)
                ?? await userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                GoogleId = googleId,
                EmailConfirmed = true,
                Language = language,
                Currency = language == "en-US" ? "USD" : "BRL",
                CreatedAt = DateTime.UtcNow,
            };
            await userRepository.AddAsync(user);
        }
        else if (user.GoogleId is null)
        {
            user.GoogleId = googleId;
            user.EmailConfirmed = true;
            await userRepository.UpdateAsync(user);
        }

        return user;
    }
}
