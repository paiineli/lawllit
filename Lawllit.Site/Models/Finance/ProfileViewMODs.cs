using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contracts;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Lawllit.Site.Models.Finance;

public sealed class ProfileViewMOD
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool HasPassword { get; set; }
    public DateTime MemberSince { get; set; }
    public string ActiveTab { get; set; } = "info";
    public string Theme { get; set; } = "dark";
    public string FontSize { get; set; } = "normal";
    public string Language { get; set; } = Constants.DefaultLanguage;
    public string Currency { get; set; } = Constants.DefaultCurrency;

    // "Set. 2025", com a primeira letra maiúscula e um único ponto no fim,
    // porque a abreviação do mês varia entre as culturas suportadas.
    public string MemberSinceDisplay
    {
        get
        {
            var monthRaw = MemberSince.ToString("MMM", CultureInfo.CurrentUICulture);
            var formattedMonth = char.ToUpper(monthRaw[0]) + monthRaw[1..].TrimEnd('.') + ".";
            return $"{formattedMonth} {MemberSince.Year}";
        }
    }

    public static ProfileViewMOD From(ProfileMOD profile, string? tab) => new()
    {
        Name = profile.Name,
        Email = profile.Email,
        HasPassword = profile.HasPassword,
        MemberSince = profile.MemberSince,
        ActiveTab = tab ?? "info",
        Theme = profile.Theme,
        FontSize = profile.FontSize,
        Language = profile.Language,
        Currency = profile.Currency,
    };
}

public sealed class EditNameViewMOD
{
    [Required(ErrorMessage = "Val_NameRequired")]
    [MaxLength(100, ErrorMessage = "Val_NameMaxLength")]
    public string Name { get; set; } = string.Empty;
}

public sealed class EditEmailViewMOD
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }
}

public sealed class ChangePasswordViewMOD
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
