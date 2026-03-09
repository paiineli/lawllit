using System.ComponentModel.DataAnnotations;

namespace Lawllit.Web.Areas.Finance.Models;

public class RegisterViewModel
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
}
