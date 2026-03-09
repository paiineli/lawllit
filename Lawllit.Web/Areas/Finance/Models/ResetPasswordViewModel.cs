using System.ComponentModel.DataAnnotations;

namespace Lawllit.Web.Areas.Finance.Models;

public class ResetPasswordViewModel
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
