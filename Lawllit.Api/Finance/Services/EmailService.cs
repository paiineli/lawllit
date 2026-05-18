using Lawllit.Api.Finance.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Lawllit.Api.Finance.Services;

public class EmailService(
    IConfiguration config,
    ILogger<EmailService> logger,
    IStringLocalizerFactory localizerFactory,
    IHttpClientFactory httpClientFactory) : IEmailService
{
    public async Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationUrl, string language)
    {
        var localizer = CreateLocalizer(language);
        var subject = localizer["Email_Confirm_Subject"].Value;
        var body = BuildEmailBody(
            heading: localizer["Email_Confirm_Heading"].Value,
            greeting: string.Format(localizer["Email_Confirm_Greeting"].Value, toName),
            actionUrl: confirmationUrl,
            actionLabel: localizer["Email_Confirm_ActionLabel"].Value,
            footerLine1: localizer["Email_Confirm_Footer1"].Value,
            footerLine2: localizer["Email_Confirm_Footer2"].Value,
            language: language
        );

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetUrl, string language)
    {
        var localizer = CreateLocalizer(language);
        var subject = localizer["Email_Reset_Subject"].Value;
        var body = BuildEmailBody(
            heading: localizer["Email_Reset_Heading"].Value,
            greeting: string.Format(localizer["Email_Reset_Greeting"].Value, toName),
            actionUrl: resetUrl,
            actionLabel: localizer["Email_Reset_ActionLabel"].Value,
            footerLine1: localizer["Email_Reset_Footer1"].Value,
            footerLine2: localizer["Email_Reset_Footer2"].Value,
            language: language
        );

        await SendEmailAsync(toEmail, subject, body);
    }

    private IStringLocalizer CreateLocalizer(string language)
    {
        CultureInfo.CurrentUICulture = new CultureInfo(language);
        return localizerFactory.Create(typeof(SharedResource));
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var fromRaw = config["Email:From"]!;
        var fromName = fromRaw.Contains('<') ? fromRaw[..fromRaw.LastIndexOf('<')].Trim() : fromRaw;
        var fromEmail = fromRaw.Contains('<') ? fromRaw[(fromRaw.LastIndexOf('<') + 1)..].TrimEnd('>').Trim() : fromRaw;

        var payload = JsonSerializer.Serialize(new
        {
            sender = new { name = fromName, email = fromEmail },
            to = new[] { new { email = toEmail } },
            subject,
            htmlContent = htmlBody
        });

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", config["Email:BrevoApiKey"]!);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Brevo API error {(int)response.StatusCode}: {error}");
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to send email to {Email} with subject {Subject}", toEmail, subject);
            throw;
        }
    }

    private static string BuildEmailBody(string heading, string greeting, string actionUrl, string actionLabel, string footerLine1, string footerLine2, string language)
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
