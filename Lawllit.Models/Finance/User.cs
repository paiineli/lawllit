namespace Lawllit.Models.Finance;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Theme { get; set; } = "dark";
    public string FontSize { get; set; } = "normal";
    public string Language { get; set; } = "pt-BR";
    public string Currency { get; set; } = "BRL";
    public bool IsOnboardingCompleted { get; set; }
}
