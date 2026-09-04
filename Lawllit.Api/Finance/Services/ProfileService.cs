using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;

namespace Lawllit.Api.Finance.Services;

public sealed class ProfileService(IUserREP userRepository) : IProfileService
{
    // Cada preferência declara os valores aceitos e como se aplica no usuário.
    // A validação fica na API para o Site não ser a única barreira.
    private static readonly Dictionary<string, (string[] ValidValues, Action<UserMOD, string> Apply)> AllowedPreferences = new()
    {
        ["theme"] = (Constants.ValidThemes, static (user, value) => user.Theme = value),
        ["fontSize"] = (Constants.ValidFontSizes, static (user, value) => user.FontSize = value),
        ["language"] = (Constants.ValidLanguages, static (user, value) => user.Language = value),
        ["currency"] = (Constants.ValidCurrencies, static (user, value) => user.Currency = value),
    };

    public async Task<ProfileMOD?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;

        return new ProfileMOD
        {
            Name = user.Name,
            Email = user.Email,
            HasPassword = user.PasswordHash is not null,
            MemberSince = user.CreatedAt,
            Theme = user.Theme,
            FontSize = user.FontSize,
            Language = user.Language,
            Currency = user.Currency,
        };
    }

    public async Task<Result<UserMOD>> EditNameAsync(Guid userId, EditNameMOD editName, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserMOD>.Failure("Msg_OperationNotAllowed");

        user.Name = StringHelpers.ToTitleCase(editName.Name);
        await userRepository.UpdateAsync(user, cancellationToken);

        return Result<UserMOD>.Success(user);
    }

    public async Task<Result<UserMOD>> EditEmailAsync(Guid userId, EditEmailMOD editEmail, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserMOD>.Failure("Msg_OperationNotAllowed");

        // Conta que entra só pelo Google não tem senha, então não pede confirmação.
        if (user.PasswordHash is not null &&
            (string.IsNullOrEmpty(editEmail.Password) || !BCrypt.Net.BCrypt.Verify(editEmail.Password, user.PasswordHash)))
            return Result<UserMOD>.Failure("Msg_WrongPassword");

        var normalizedEmail = editEmail.Email.Trim().ToLower();

        if (user.Email == normalizedEmail)
            return Result<UserMOD>.Failure("Msg_EmailSameAsCurrent");

        var existingUser = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
            return Result<UserMOD>.Failure("Msg_EmailInUse");

        user.Email = normalizedEmail;
        await userRepository.UpdateAsync(user, cancellationToken);

        return Result<UserMOD>.Success(user);
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordMOD changePassword, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.PasswordHash is null)
            return Result.Failure("Msg_OperationNotAllowed");

        if (!BCrypt.Net.BCrypt.Verify(changePassword.CurrentPassword, user.PasswordHash))
            return Result.Failure("Msg_CurrentPasswordWrong");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePassword.NewPassword);
        await userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserMOD>> SavePreferenceAsync(Guid userId, PreferenceMOD preference, CancellationToken cancellationToken)
    {
        if (!AllowedPreferences.TryGetValue(preference.Key, out var allowed) || !allowed.ValidValues.Contains(preference.Value))
            return Result<UserMOD>.Failure("Msg_DataInvalid");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserMOD>.Failure("Msg_OperationNotAllowed");

        allowed.Apply(user, preference.Value);
        await userRepository.UpdateAsync(user, cancellationToken);

        return Result<UserMOD>.Success(user);
    }

    public async Task<Result> DeleteAccountAsync(Guid userId, DeleteAccountMOD deleteAccount, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure("Msg_OperationNotAllowed");

        if (user.PasswordHash is not null && !BCrypt.Net.BCrypt.Verify(deleteAccount.Password ?? string.Empty, user.PasswordHash))
            return Result.Failure("Msg_WrongPassword");

        // Categorias e transações saem por ON DELETE CASCADE declarado no schema.
        await userRepository.DeleteAsync(userId, cancellationToken);

        return Result.Success();
    }
}

#region Interfaces

public interface IProfileService
{
    Task<ProfileMOD?> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<UserMOD>> EditNameAsync(Guid userId, EditNameMOD editName, CancellationToken cancellationToken);
    Task<Result<UserMOD>> EditEmailAsync(Guid userId, EditEmailMOD editEmail, CancellationToken cancellationToken);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordMOD changePassword, CancellationToken cancellationToken);
    Task<Result<UserMOD>> SavePreferenceAsync(Guid userId, PreferenceMOD preference, CancellationToken cancellationToken);
    Task<Result> DeleteAccountAsync(Guid userId, DeleteAccountMOD deleteAccount, CancellationToken cancellationToken);
}

#endregion
