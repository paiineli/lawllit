using System.ComponentModel.DataAnnotations;

namespace Lawllit.Site.Models.Finance;

public sealed class LoginViewMOD
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_PasswordRequired")]
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterViewMOD
{
    [Required(ErrorMessage = "Val_NameRequired")]
    [StringLength(100, ErrorMessage = "Val_NameMaxLength")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_PasswordRequired")]
    [MinLength(6, ErrorMessage = "Val_PasswordMin6")]
    public string Password { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Val_TermsRequired")]
    public bool AcceptedTerms { get; set; }
}

public sealed class ForgotPasswordViewMOD
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordViewMOD
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_PasswordRequired")]
    [MinLength(6, ErrorMessage = "Val_Min6Chars")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_ConfirmRequired")]
    [Compare(nameof(Password), ErrorMessage = "Val_PasswordMismatch")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
