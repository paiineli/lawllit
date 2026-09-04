using Lawllit.Api.Common;
using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;

namespace Lawllit.Api.Finance.Services;

public sealed class AuthService(
    IUserREP userRepository,
    IEmailService emailService,
    ITokenGenerator tokenGenerator) : IAuthService
{
    public async Task<Result<AuthResultMOD>> LoginAsync(LoginMOD login, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(login.Email.Trim().ToLower(), cancellationToken);

        if (user is null || user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            return Result<AuthResultMOD>.Failure("Msg_WrongCredentials");

        if (!user.EmailConfirmed)
            return Result<AuthResultMOD>.Failure("Msg_EmailNotConfirmed");

        return Result<AuthResultMOD>.Success(BuildAuthResult(user));
    }

    public async Task<Result> RegisterAsync(RegisterMOD register, CancellationToken cancellationToken)
    {
        var normalizedEmail = register.Email.Trim().ToLower();

        var existingUser = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
            return Result.Failure("Msg_EmailInUse");

        var user = new UserMOD
        {
            Id = Guid.NewGuid(),
            Name = StringHelpers.ToTitleCase(register.Name),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
            EmailConfirmed = false,
            EmailConfirmationToken = Guid.NewGuid().ToString("N"),
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            Language = register.Language,
            Currency = register.Language == "en-US" ? "USD" : Constants.DefaultCurrency,
        };

        await userRepository.AddAsync(user, cancellationToken);

        await emailService.SendConfirmationEmailAsync(
            user,
            register.ConfirmationUrlTemplate.Replace("{token}", user.EmailConfirmationToken),
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<AuthResultMOD>> ConfirmEmailAsync(string token, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByConfirmationTokenAsync(token, cancellationToken);

        if (user is null || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            return Result<AuthResultMOD>.Failure("Msg_InvalidLink");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;
        await userRepository.UpdateAsync(user, cancellationToken);

        return Result<AuthResultMOD>.Success(BuildAuthResult(user));
    }

    public async Task<bool> IsPasswordResetTokenValidAsync(string token, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(token, cancellationToken);
        return user is not null && user.PasswordResetTokenExpiry >= DateTime.UtcNow;
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordMOD forgotPassword, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(forgotPassword.Email.Trim().ToLower(), cancellationToken);

        // Conta inexistente ou sem e-mail confirmado não gera token. O Site mostra a
        // mesma mensagem genérica nos dois casos, para não revelar quem tem cadastro.
        if (user is null || !user.EmailConfirmed)
            return Result.Failure("Msg_NotFound");

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await userRepository.UpdateAsync(user, cancellationToken);

        await emailService.SendPasswordResetEmailAsync(
            user,
            forgotPassword.ResetUrlTemplate.Replace("{token}", user.PasswordResetToken),
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordMOD resetPassword, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(resetPassword.Token, cancellationToken);

        if (user is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return Result.Failure("Msg_InvalidLink");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPassword.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }

    public async Task<AuthResultMOD> LoginWithGoogleAsync(GoogleUserMOD googleUser, CancellationToken cancellationToken)
    {
        var normalizedEmail = googleUser.Email.Trim().ToLower();

        var user = await userRepository.GetByGoogleIdAsync(googleUser.GoogleId, cancellationToken)
                ?? await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new UserMOD
            {
                Id = Guid.NewGuid(),
                Name = StringHelpers.ToTitleCase(googleUser.Name),
                Email = normalizedEmail,
                GoogleId = googleUser.GoogleId,
                EmailConfirmed = true,
                Language = googleUser.Language,
                Currency = googleUser.Language == "en-US" ? "USD" : Constants.DefaultCurrency,
                CreatedAt = DateTime.UtcNow,
            };

            await userRepository.AddAsync(user, cancellationToken);
        }
        else if (user.GoogleId is null)
        {
            // Conta criada com senha entrando pelo Google pela primeira vez, vincula as duas.
            user.GoogleId = googleUser.GoogleId;
            user.EmailConfirmed = true;
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        return BuildAuthResult(user);
    }

    private AuthResultMOD BuildAuthResult(UserMOD user)
    {
        var token = tokenGenerator.Generate(user);

        return new AuthResultMOD
        {
            User = user,
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
        };
    }
}

#region Interfaces

public interface IAuthService
{
    Task<Result<AuthResultMOD>> LoginAsync(LoginMOD login, CancellationToken cancellationToken);
    Task<Result> RegisterAsync(RegisterMOD register, CancellationToken cancellationToken);
    Task<Result<AuthResultMOD>> ConfirmEmailAsync(string token, CancellationToken cancellationToken);
    Task<bool> IsPasswordResetTokenValidAsync(string token, CancellationToken cancellationToken);
    Task<Result> ForgotPasswordAsync(ForgotPasswordMOD forgotPassword, CancellationToken cancellationToken);
    Task<Result> ResetPasswordAsync(ResetPasswordMOD resetPassword, CancellationToken cancellationToken);
    Task<AuthResultMOD> LoginWithGoogleAsync(GoogleUserMOD googleUser, CancellationToken cancellationToken);
}

#endregion
