namespace Lawllit.Api.Finance.Services.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationUrl, string language);
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetUrl, string language);
}
