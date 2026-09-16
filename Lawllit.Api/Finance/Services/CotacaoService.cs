using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contratos;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lawllit.Api.Finance.Services;

public sealed class CotacaoService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<CotacaoService> logger) : ICotacaoService
{
    private const string ChaveCache = "cotacoes";
    private const string MensagemErro = "Não foi possível carregar as cotações agora.";

    private static readonly TimeSpan CacheSucesso = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CacheFalha = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan TempoLimite = TimeSpan.FromSeconds(10);

    private static readonly JsonSerializerOptions OpcoesLeitura = new() { PropertyNameCaseInsensitive = true };

    // Ordem e casas decimais de cada moeda exibida. A chave é a do retorno da AwesomeAPI.
    private static readonly (string ChaveApi, string Rotulo, string Bandeira, int CasasDecimais)[] Moedas =
    [
        ("USDBRL", "Dólar",           "\U0001F1FA\U0001F1F8", 4),
        ("EURBRL", "Euro",            "\U0001F1EA\U0001F1FA", 4),
        ("GBPBRL", "Libra",           "\U0001F1EC\U0001F1E7", 4),
        ("JPYBRL", "Iene",            "\U0001F1EF\U0001F1F5", 4),
        ("BTCBRL", "Bitcoin",         "₿",               2),
        ("ARSBRL", "Peso Argentino",  "\U0001F1E6\U0001F1F7", 4),
    ];

    public async Task<Resultado<List<CotacaoModel>>> Listar(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(ChaveCache, out Resultado<List<CotacaoModel>>? emCache) && emCache is not null)
            return emCache;

        var resultado = await Consultar(cancellationToken);

        // Falha fica menos tempo em cache, para tentar de novo sem martelar a API externa.
        cache.Set(ChaveCache, resultado, resultado.Sucesso ? CacheSucesso : CacheFalha);

        return resultado;
    }

    private async Task<Resultado<List<CotacaoModel>>> Consultar(CancellationToken cancellationToken)
    {
        var url = configuration["Api:ExchangeRateUrl"];

        if (string.IsNullOrWhiteSpace(url))
        {
            logger.LogError("Api__ExchangeRateUrl não configurada, cotações indisponíveis.");
            return Resultado<List<CotacaoModel>>.Falha(MensagemErro);
        }

        Dictionary<string, CotacaoApiModel>? retorno;

        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TempoLimite;
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LawllitFinance/1.0");

            var json = await httpClient.GetStringAsync(url, cancellationToken);
            retorno = JsonSerializer.Deserialize<Dictionary<string, CotacaoApiModel>>(json, OpcoesLeitura);
        }
        catch (Exception excecao) when (excecao is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogError(excecao, "Falha ao consultar as cotações em {Url}", url);
            return Resultado<List<CotacaoModel>>.Falha(MensagemErro);
        }

        if (retorno is null || retorno.Count == 0)
        {
            logger.LogError("A API de cotações respondeu sem nenhuma moeda.");
            return Resultado<List<CotacaoModel>>.Falha(MensagemErro);
        }

        var fusoBrasil = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var cotacoes = new List<CotacaoModel>(Moedas.Length);

        foreach (var moeda in Moedas)
        {
            if (!retorno.TryGetValue(moeda.ChaveApi, out var dados)) continue;

            if (!TentarConverter(dados.Bid, out var valorCompra) ||
                !TentarConverter(dados.High, out var maximaDia) ||
                !TentarConverter(dados.Low, out var minimaDia))
            {
                logger.LogWarning("Cotação de {Moeda} veio incompleta e foi ignorada.", moeda.ChaveApi);
                continue;
            }

            TentarConverter(dados.PctChange, out var variacao);

            var atualizadoUtc = long.TryParse(dados.Timestamp, out var segundosUnix)
                ? DateTimeOffset.FromUnixTimeSeconds(segundosUnix).UtcDateTime
                : DateTime.UtcNow;

            cotacoes.Add(new CotacaoModel
            {
                Rotulo = moeda.Rotulo,
                Bandeira = moeda.Bandeira,
                ValorCompra = valorCompra,
                MaximaDia = maximaDia,
                MinimaDia = minimaDia,
                VariacaoPercentual = variacao,
                CasasDecimais = moeda.CasasDecimais,
                AtualizadoEm = TimeZoneInfo.ConvertTimeFromUtc(atualizadoUtc, fusoBrasil),
            });
        }

        return cotacoes.Count > 0
            ? Resultado<List<CotacaoModel>>.Ok(cotacoes)
            : Resultado<List<CotacaoModel>>.Falha(MensagemErro);
    }

    private static bool TentarConverter(string? valor, out decimal convertido)
        => decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out convertido);
}

// Formato cru da AwesomeAPI, só existe dentro da API porque o Site consome o CotacaoModel.
internal sealed class CotacaoApiModel
{
    [JsonPropertyName("bid")]
    public string Bid { get; set; } = string.Empty;

    [JsonPropertyName("high")]
    public string High { get; set; } = string.Empty;

    [JsonPropertyName("low")]
    public string Low { get; set; } = string.Empty;

    [JsonPropertyName("pctChange")]
    public string PctChange { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}

#region Interfaces

public interface ICotacaoService
{
    Task<Resultado<List<CotacaoModel>>> Listar(CancellationToken cancellationToken);
}

#endregion
