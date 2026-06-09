using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance;
using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services;

public class ProfileService(IUserRepository userRepository) : IProfileService
{
    public async Task<ProfileViewModel?> GetProfileAsync(Guid userId, string? tab)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        return new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email,
            HasPassword = user.PasswordHash is not null,
            MemberSince = user.CreatedAt,
            ActiveTab = tab ?? "info",
            Theme = user.Theme,
            FontSize = user.FontSize,
            Language = user.Language,
            Currency = user.Currency,
        };
    }

    public async Task<Result<User>> EditNameAsync(Guid userId, string name)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result<User>.Failure("Msg_OperationNotAllowed");

        user.Name = StringHelpers.ToTitleCase(name);
        await userRepository.UpdateAsync(user);
        return Result<User>.Success(user);
    }

    public async Task<Result<User>> EditEmailAsync(Guid userId, EditEmailViewModel form)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result<User>.Failure("Msg_OperationNotAllowed");

        if (user.PasswordHash is not null &&
            (string.IsNullOrEmpty(form.Password) || !BCrypt.Net.BCrypt.Verify(form.Password, user.PasswordHash)))
            return Result<User>.Failure("Msg_WrongPassword");

        var normalizedEmail = form.Email.Trim().ToLower();

        if (user.Email == normalizedEmail)
            return Result<User>.Failure("Msg_EmailSameAsCurrent");

        var existingUser = await userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser is not null)
            return Result<User>.Failure("Msg_EmailInUse");

        user.Email = normalizedEmail;
        await userRepository.UpdateAsync(user);
        return Result<User>.Success(user);
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordViewModel form)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null || user.PasswordHash is null)
            return Result.Failure("Msg_OperationNotAllowed");

        if (!BCrypt.Net.BCrypt.Verify(form.CurrentPassword, user.PasswordHash))
            return Result.Failure("Msg_CurrentPasswordWrong");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(form.NewPassword);
        await userRepository.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result<User>> UpdatePreferenceAsync(Guid userId, Action<User> applyUpdate)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result<User>.Failure("Msg_OperationNotAllowed");

        applyUpdate(user);
        await userRepository.UpdateAsync(user);
        return Result<User>.Success(user);
    }

    public async Task<Result> DeleteAccountAsync(Guid userId, string? password)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure("Msg_OperationNotAllowed");

        if (user.PasswordHash is not null && !BCrypt.Net.BCrypt.Verify(password ?? "", user.PasswordHash))
            return Result.Failure("Msg_WrongPassword");

        await userRepository.DeleteAsync(userId);
        return Result.Success();
    }
}
