using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Lawllit.Api.Common;

// Fora de Finance porque o contato do portfólio usa o mesmo envio.
public sealed class EnviadorEmail(
    IConfiguration configuration,
    ILogger<EnviadorEmail> logger,
    IHttpClientFactory httpClientFactory) : IEnviadorEmail
{
    private const string EndpointResend = "https://api.resend.com/emails";

    public async Task Enviar(
        string destinatario,
        string assunto,
        string corpoHtml,
        CancellationToken cancellationToken,
        string? responderPara = null)
    {
        var remetente = configuration["Email:From"]
            ?? throw new InvalidOperationException("Email__From não configurado no ambiente.");

        var chaveApi = configuration["Email:ApiKey"]
            ?? throw new InvalidOperationException("Email__ApiKey não configurada no ambiente.");

        var corpo = new Dictionary<string, object>
        {
            ["from"] = remetente,
            ["to"] = new[] { destinatario },
            ["subject"] = assunto,
            ["html"] = corpoHtml,
        };

        // Sem isto, responder o aviso iria para o remetente do sistema, e não para quem escreveu.
        if (!string.IsNullOrWhiteSpace(responderPara))
            corpo["reply_to"] = responderPara;

        using var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", chaveApi);

        using var resposta = await httpClient.PostAsJsonAsync(EndpointResend, corpo, cancellationToken);

        if (resposta.IsSuccessStatusCode) return;

        var erro = await resposta.Content.ReadAsStringAsync(cancellationToken);
        logger.LogError("Resend recusou o envio para {Email}. Status {Status}. Corpo {Body}",
            destinatario, (int)resposta.StatusCode, erro);

        throw new InvalidOperationException($"Resend respondeu {(int)resposta.StatusCode} ao enviar o e-mail.");
    }
}

#region Interfaces

public interface IEnviadorEmail
{
    Task Enviar(
        string destinatario,
        string assunto,
        string corpoHtml,
        CancellationToken cancellationToken,
        string? responderPara = null);
}

#endregion
