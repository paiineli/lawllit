using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contracts;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lawllit.Api.Finance.Services;

public sealed class QuotesService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<QuotesService> logger) : IQuotesService
{
    private const string CacheKey = "quotes";
    private static readonly TimeSpan SuccessCacheDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan FailureCacheDuration = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    private static readonly JsonSerializerOptions DeserializeOptions = new() { PropertyNameCaseInsensitive = true };

    // Ordem e casas decimais de cada moeda exibida. A chave é a do retorno da AwesomeAPI.
    private static readonly (string ApiKey, string LabelKey, string FlagEmoji, int DecimalPlaces)[] Currencies =
    [
        ("USDBRL", "Currency_Dollar",    "\U0001F1FA\U0001F1F8", 4),
        ("EURBRL", "Currency_Euro",      "\U0001F1EA\U0001F1FA", 4),
        ("GBPBRL", "Currency_Pound",     "\U0001F1EC\U0001F1E7", 4),
        ("JPYBRL", "Currency_Yen",       "\U0001F1EF\U0001F1F5", 4),
        ("BTCBRL", "Currency_Bitcoin",   "₿",               2),
        ("ARSBRL", "Currency_Argentine", "\U0001F1E6\U0001F1F7", 4),
    ];

    public async Task<Result<List<QuoteMOD>>> GetQuotesAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out Result<List<QuoteMOD>>? cached) && cached is not null)
            return cached;

        var result = await FetchQuotesAsync(cancellationToken);

        // Falha fica em cache por menos tempo, para a próxima visita já tentar de novo
        // sem martelar a API externa a cada F5.
        cache.Set(CacheKey, result, result.IsSuccess ? SuccessCacheDuration : FailureCacheDuration);

        return result;
    }

    private async Task<Result<List<QuoteMOD>>> FetchQuotesAsync(CancellationToken cancellationToken)
    {
        var exchangeRateUrl = configuration["Api:ExchangeRateUrl"];

        if (string.IsNullOrWhiteSpace(exchangeRateUrl))
        {
            logger.LogError("Api__ExchangeRateUrl não configurada, cotações indisponíveis.");
            return Result<List<QuoteMOD>>.Failure("Quote_LoadError");
        }

        Dictionary<string, QuoteApiDataMOD>? payload;

        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = RequestTimeout;
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LawllitFinance/1.0");

            var json = await httpClient.GetStringAsync(exchangeRateUrl, cancellationToken);
            payload = JsonSerializer.Deserialize<Dictionary<string, QuoteApiDataMOD>>(json, DeserializeOptions);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogError(exception, "Falha ao consultar as cotações em {Url}", exchangeRateUrl);
            return Result<List<QuoteMOD>>.Failure("Quote_LoadError");
        }

        if (payload is null || payload.Count == 0)
        {
            logger.LogError("A API de cotações respondeu sem nenhuma moeda.");
            return Result<List<QuoteMOD>>.Failure("Quote_LoadError");
        }

        var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var quotes = new List<QuoteMOD>(Currencies.Length);

        foreach (var currency in Currencies)
        {
            if (!payload.TryGetValue(currency.ApiKey, out var data)) continue;

            if (!TryParseDecimal(data.Bid, out var buyRate) ||
                !TryParseDecimal(data.High, out var dailyHigh) ||
                !TryParseDecimal(data.Low, out var dailyLow))
            {
                logger.LogWarning("Cotação de {Currency} veio incompleta e foi ignorada.", currency.ApiKey);
                continue;
            }

            TryParseDecimal(data.PctChange, out var pctChange);

            var lastUpdatedUtc = long.TryParse(data.Timestamp, out var unixSeconds)
                ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime
                : DateTime.UtcNow;

            quotes.Add(new QuoteMOD
            {
                LabelKey = currency.LabelKey,
                FlagEmoji = currency.FlagEmoji,
                BuyRate = buyRate,
                DailyHigh = dailyHigh,
                DailyLow = dailyLow,
                PctChange = pctChange,
                DecimalPlaces = currency.DecimalPlaces,
                LastUpdated = TimeZoneInfo.ConvertTimeFromUtc(lastUpdatedUtc, brazilTimeZone),
            });
        }

        return quotes.Count > 0
            ? Result<List<QuoteMOD>>.Success(quotes)
            : Result<List<QuoteMOD>>.Failure("Quote_LoadError");
    }

    private static bool TryParseDecimal(string? value, out decimal parsed)
        => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed);
}

// Formato cru da AwesomeAPI, só existe dentro da API porque o Site consome o QuoteMOD.
internal sealed class QuoteApiDataMOD
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

public interface IQuotesService
{
    Task<Result<List<QuoteMOD>>> GetQuotesAsync(CancellationToken cancellationToken);
}

#endregion
