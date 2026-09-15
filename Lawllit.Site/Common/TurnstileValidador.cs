using System.Text.Json.Serialization;

namespace Lawllit.Site.Common;

// Valida no Site, e não na API, para o robô nem chegar a gastar uma chamada de API.
public sealed class TurnstileValidador(
    IConfiguration configuration,
    ILogger<TurnstileValidador> logger,
    IHttpClientFactory httpClientFactory) : ITurnstileValidador
{
    private const string EndpointVerificacao = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public async Task<bool> Validar(string? token, string? ipRemoto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        var chaveSecreta = configuration[TurnstileChaves.CaminhoChaveSecreta];

        if (string.IsNullOrWhiteSpace(chaveSecreta))
            throw new InvalidOperationException("Turnstile__SecretKey não configurada no ambiente.");

        var campos = new Dictionary<string, string>
        {
            ["secret"] = chaveSecreta,
            ["response"] = token,
        };

        // O IP serve só para a Cloudflare cruzar com a reputação dela. Não é guardado aqui.
        if (!string.IsNullOrWhiteSpace(ipRemoto))
            campos["remoteip"] = ipRemoto;

        using var httpClient = httpClientFactory.CreateClient();
        using var conteudo = new FormUrlEncodedContent(campos);

        try
        {
            using var resposta = await httpClient.PostAsync(EndpointVerificacao, conteudo, cancellationToken);

            if (!resposta.IsSuccessStatusCode)
            {
                logger.LogWarning("Turnstile respondeu {Status} na verificação.", (int)resposta.StatusCode);
                return false;
            }

            var retorno = await resposta.Content.ReadFromJsonAsync<TurnstileRespostaModel>(cancellationToken);

            if (retorno?.Success == true) return true;

            logger.LogWarning("Turnstile recusou o token. Códigos {Codigos}", string.Join(", ", retorno?.ErrorCodes ?? []));
            return false;
        }
        catch (HttpRequestException excecao)
        {
            // Cloudflare fora do ar recusa o envio. Formulário aberto vira alvo de robô em horas.
            logger.LogError(excecao, "Não foi possível falar com o Turnstile.");
            return false;
        }
    }

    // Os nomes seguem o retorno da Cloudflare, por isso continuam em inglês.
    private sealed class TurnstileRespostaModel
    {
        public bool Success { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}

#region Interfaces

public interface ITurnstileValidador
{
    Task<bool> Validar(string? token, string? ipRemoto, CancellationToken cancellationToken);
}

#endregion
