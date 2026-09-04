using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Lawllit.Api.Finance.Services;

public sealed class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger,
    IStringLocalizerFactory localizerFactory,
    IHttpClientFactory httpClientFactory) : IEmailService
{
    private const string BrevoEndpoint = "https://api.brevo.com/v3/smtp/email";

    public Task SendConfirmationEmailAsync(UserMOD user, string confirmationUrl, CancellationToken cancellationToken)
    {
        var localizer = CreateLocalizer(user.Language);

        var body = BuildEmailBody(
            heading: localizer["Email_Confirm_Heading"].Value,
            greeting: string.Format(localizer["Email_Confirm_Greeting"].Value, user.Name),
            actionUrl: confirmationUrl,
            actionLabel: localizer["Email_Confirm_ActionLabel"].Value,
            footerLine1: localizer["Email_Confirm_Footer1"].Value,
            footerLine2: localizer["Email_Confirm_Footer2"].Value,
            language: user.Language);

        return SendAsync(user.Email, localizer["Email_Confirm_Subject"].Value, body, cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(UserMOD user, string resetUrl, CancellationToken cancellationToken)
    {
        var localizer = CreateLocalizer(user.Language);

        var body = BuildEmailBody(
            heading: localizer["Email_Reset_Heading"].Value,
            greeting: string.Format(localizer["Email_Reset_Greeting"].Value, user.Name),
            actionUrl: resetUrl,
            actionLabel: localizer["Email_Reset_ActionLabel"].Value,
            footerLine1: localizer["Email_Reset_Footer1"].Value,
            footerLine2: localizer["Email_Reset_Footer2"].Value,
            language: user.Language);

        return SendAsync(user.Email, localizer["Email_Reset_Subject"].Value, body, cancellationToken);
    }

    // O e-mail sai no idioma do destinatário, não no da requisição que disparou o envio.
    private IStringLocalizer CreateLocalizer(string language)
    {
        CultureInfo.CurrentUICulture = new CultureInfo(language);
        return localizerFactory.Create(typeof(SharedResource));
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var senderRaw = configuration["Email:From"]
            ?? throw new InvalidOperationException("Email__From não configurado no ambiente.");

        var hasDisplayName = senderRaw.Contains('<');
        var senderName = hasDisplayName ? senderRaw[..senderRaw.LastIndexOf('<')].Trim() : senderRaw;
        var senderEmail = hasDisplayName ? senderRaw[(senderRaw.LastIndexOf('<') + 1)..].TrimEnd('>').Trim() : senderRaw;

        var payload = JsonSerializer.Serialize(new
        {
            sender = new { name = senderName, email = senderEmail },
            to = new[] { new { email = toEmail } },
            subject,
            htmlContent = htmlBody,
        });

        using var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("api-key", configuration["Email:BrevoApiKey"]
            ?? throw new InvalidOperationException("Email__BrevoApiKey não configurada no ambiente."));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await httpClient.PostAsync(BrevoEndpoint, content, cancellationToken);

        if (response.IsSuccessStatusCode) return;

        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogError("Brevo recusou o envio para {Email}. Status {Status}. Corpo {Body}", toEmail, (int)response.StatusCode, errorBody);

        throw new InvalidOperationException($"Brevo respondeu {(int)response.StatusCode} ao enviar o e-mail.");
    }

    private static string BuildEmailBody(
        string heading,
        string greeting,
        string actionUrl,
        string actionLabel,
        string footerLine1,
        string footerLine2,
        string language)
        => $"""
            <!DOCTYPE html>
            <html lang="{language}">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width"></head>
            <body style="margin:0;padding:0;background-color:#0a0a0a;font-family:Menlo,Monaco,Consolas,'Courier New',monospace;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#0a0a0a;padding:48px 20px;">
                <tr>
                  <td align="center">
                    <table width="100%" cellpadding="0" cellspacing="0" style="max-width:480px;">

                      <tr>
                        <td align="center" style="padding-bottom:28px;">
                          <span style="font-size:20px;font-weight:700;color:#e5e7eb;letter-spacing:-0.3px;">
                            lawllit<span style="color:#4ade80;">finance</span>
                          </span>
                        </td>
                      </tr>

                      <tr>
                        <td style="background-color:#111111;border:1px solid rgba(255,255,255,0.08);border-radius:12px;padding:40px 36px;">
                          <h2 style="margin:0 0 8px 0;font-size:18px;font-weight:600;color:#e5e7eb;">{heading}</h2>
                          <p style="margin:0 0 32px 0;font-size:14px;color:#6b7280;line-height:1.6;">{greeting}</p>
                          <table width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                              <td align="center">
                                <a href="{actionUrl}"
                                   style="display:inline-block;padding:13px 36px;background-color:#4ade80;color:#000000;font-weight:700;font-size:14px;text-decoration:none;border-radius:6px;letter-spacing:-0.2px;">
                                  {actionLabel}
                                </a>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:24px 0 0 0;text-align:center;">
                          <p style="margin:0 0 4px 0;font-size:12px;color:#6b7280;">{footerLine1}</p>
                          <p style="margin:0;font-size:12px;color:#6b7280;">{footerLine2}</p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
}

#region Interfaces

public interface IEmailService
{
    Task SendConfirmationEmailAsync(UserMOD user, string confirmationUrl, CancellationToken cancellationToken);
    Task SendPasswordResetEmailAsync(UserMOD user, string resetUrl, CancellationToken cancellationToken);
}

#endregion
