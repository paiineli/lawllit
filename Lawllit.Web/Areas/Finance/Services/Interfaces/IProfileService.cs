using Lawllit.Data.Finance.Entities;
using Lawllit.Web.Areas.Finance.Models;

namespace Lawllit.Web.Areas.Finance.Services.Interfaces;

public interface IProfileService
{
    Task<ProfileViewModel?> GetProfileAsync(Guid userId, string? tab);
    Task<Result<User>> EditNameAsync(Guid userId, string name);
    Task<Result<User>> EditEmailAsync(Guid userId, EditEmailViewModel form);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordViewModel form);
    Task<Result<User>> SaveThemeAsync(Guid userId, string theme);
    Task<Result<User>> SaveFontSizeAsync(Guid userId, string fontSize);
    Task<Result<User>> SaveLanguageAsync(Guid userId, string language);
    Task<Result> DeleteAccountAsync(Guid userId, string? password);
}
