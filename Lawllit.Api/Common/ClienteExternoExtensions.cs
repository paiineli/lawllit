using System.Net;
using System.Text.Json;

namespace Lawllit.Api.Common;

public static class ClienteExternoExtensions
{
    public const string NomeCliente = "Externo";

    // A Liquipedia bloqueia User-Agent genérico e exige um com o projeto e o contato. As
    // outras fontes não pedem, mas recebem o mesmo para saberem quem está chamando.
    private const string UserAgent = "Lawllit/1.0 (https://lawllit.com; contato@lawllit.com)";

    public static IServiceCollection AddClienteExterno(this IServiceCollection services)
    {
        services.AddHttpClient(NomeCliente, httpClient =>
            {
                httpClient.Timeout = TimeSpan.FromSeconds(20);
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            })
            // A Liquipedia recusa quem não aceita gzip, e o handler já manda o Accept-Encoding.
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            });

        return services;
    }

    // Nulo quando a fonte cai ou responde fora do formato, com o motivo no log. Lê como texto
    // porque a página de bloqueio da Liquipedia chega em HTML, e aí o erro é de JSON.
    public static async Task<TValor?> LerJson<TValor>(
        this IHttpClientFactory httpClientFactory,
        string url,
        ILogger logger,
        CancellationToken cancellationToken) where TValor : class
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient(NomeCliente);
            var json = await httpClient.GetStringAsync(url, cancellationToken);

            return JsonSerializer.Deserialize<TValor>(json);
        }
        catch (Exception excecao) when (excecao is HttpRequestException or JsonException
            || (excecao is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogError(excecao, "Falha ao consultar {Url}", url);
            return null;
        }
    }

    public static async Task<bool> Existe(
        this IHttpClientFactory httpClientFactory,
        string url,
        CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient(NomeCliente);
            using var resposta = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return resposta.IsSuccessStatusCode;
        }
        catch (Exception excecao) when (excecao is HttpRequestException
            || (excecao is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            return false;
        }
    }
}
