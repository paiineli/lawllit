using Lawllit.Model.Common;

namespace Lawllit.Model.Finance.Contracts;

public sealed class ProfileMOD
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool HasPassword { get; set; }
    public DateTime MemberSince { get; set; }
    public string Theme { get; set; } = "dark";
    public string FontSize { get; set; } = "normal";
    public string Language { get; set; } = Constants.DefaultLanguage;
    public string Currency { get; set; } = Constants.DefaultCurrency;
}

public sealed class EditNameMOD
{
    public string Name { get; set; } = string.Empty;
}

public sealed class EditEmailMOD
{
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public sealed class ChangePasswordMOD
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class PreferenceMOD
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class DeleteAccountMOD
{
    public string? Password { get; set; }
}
