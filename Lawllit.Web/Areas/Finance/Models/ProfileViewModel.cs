using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Lawllit.Web.Areas.Finance.Models;

public class ProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool HasPassword { get; set; }
    public DateTime MemberSince { get; set; }
    public string ActiveTab { get; set; } = "info";
    public string Theme { get; set; } = "dark";
    public string FontSize { get; set; } = "normal";
    public string Language { get; set; } = "pt-BR";

    public string MemberSinceDisplay
    {
        get
        {
            var monthRaw = MemberSince.ToString("MMM", CultureInfo.CurrentUICulture);
            var formattedMonth = char.ToUpper(monthRaw[0]) + monthRaw[1..].TrimEnd('.') + ".";
            return $"{formattedMonth} {MemberSince.Year}";
        }
    }
}

public class SaveThemeViewModel
{
    [Required]
    public string Theme { get; set; } = string.Empty;
}

public class SaveFontSizeViewModel
{
    [Required]
    public string FontSize { get; set; } = string.Empty;
}

public class SaveLanguageViewModel
{
    [Required]
    public string Language { get; set; } = string.Empty;
}

public class EditNameViewModel
{
    [Required(ErrorMessage = "Val_NameRequired")]
    [MaxLength(100, ErrorMessage = "Val_NameMaxLength")]
    public string Name { get; set; } = string.Empty;
}

public class EditEmailViewModel
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Val_PasswordRequired")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_PasswordRequired")]
    [MinLength(6, ErrorMessage = "Val_PasswordMin6")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_ConfirmRequired")]
    [Compare(nameof(NewPassword), ErrorMessage = "Val_PasswordMismatch")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
