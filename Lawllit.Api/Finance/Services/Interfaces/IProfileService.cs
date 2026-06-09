using Lawllit.Models.Finance;
using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services.Interfaces;

public interface IProfileService
{
    Task<ProfileViewModel?> GetProfileAsync(Guid userId, string? tab);
    Task<Result<User>> EditNameAsync(Guid userId, string name);
    Task<Result<User>> EditEmailAsync(Guid userId, EditEmailViewModel form);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordViewModel form);
    Task<Result<User>> UpdatePreferenceAsync(Guid userId, Action<User> applyUpdate);
    Task<Result> DeleteAccountAsync(Guid userId, string? password);
}
