using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Lawllit.Api.Finance.Services;

public class QuotesService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration configuration,
    IStringLocalizer<SharedResource> localizer) : IQuotesService
{
    private const string CacheKey = "quotes_data";
    private const string ErrorCacheKey = "quotes_error";

    public async Task<(List<QuoteViewModel> Quotes, string? ErrorTechDetails)> GetQuotesAsync()
    {
        if (cache.TryGetValue(CacheKey, out List<QuoteViewModel>? cachedQuotes) && cachedQuotes is not null)
        {
            string? cachedError = null;
            if (!cachedQuotes.Any())
                cache.TryGetValue(ErrorCacheKey, out cachedError);
            return (cachedQuotes, cachedError);
        }

        var quotes = new List<QuoteViewModel>();

        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LawllitFinance/1.0");

            var json = await httpClient.GetStringAsync(configuration["Api:ExchangeRateUrl"]!);
            var deserializeOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, QuoteApiData>>(json, deserializeOptions)!;

            var currencyConfigurations = new[]
            {
                new { Key = "USDBRL", LabelKey = "Currency_Dollar",    FlagEmoji = "🇺🇸", DecimalPlaces = 4 },
                new { Key = "EURBRL", LabelKey = "Currency_Euro",      FlagEmoji = "🇪🇺", DecimalPlaces = 4 },
                new { Key = "GBPBRL", LabelKey = "Currency_Pound",     FlagEmoji = "🇬🇧", DecimalPlaces = 4 },
                new { Key = "JPYBRL", LabelKey = "Currency_Yen",       FlagEmoji = "🇯🇵", DecimalPlaces = 4 },
                new { Key = "BTCBRL", LabelKey = "Currency_Bitcoin",   FlagEmoji = "₿",   DecimalPlaces = 2 },
                new { Key = "ARSBRL", LabelKey = "Currency_Argentine", FlagEmoji = "🇦🇷", DecimalPlaces = 4 },
            };

            foreach (var item in currencyConfigurations)
            {
                if (!data.TryGetValue(item.Key, out var currencyData)) continue;
                if (string.IsNullOrEmpty(currencyData.Bid) || string.IsNullOrEmpty(currencyData.High) || string.IsNullOrEmpty(currencyData.Low)) continue;

                var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                var lastUpdated = long.TryParse(currencyData.Timestamp, out var unixSeconds)
                    ? TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime, brazilTimeZone)
                    : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);

                decimal.TryParse(currencyData.PctChange, NumberStyles.Any, CultureInfo.InvariantCulture, out var pctChange);

                quotes.Add(new QuoteViewModel
                {
                    Label = localizer[item.LabelKey].Value,
                    FlagEmoji = item.FlagEmoji,
                    BuyRate = decimal.Parse(currencyData.Bid, CultureInfo.InvariantCulture),
                    DailyHigh = decimal.Parse(currencyData.High, CultureInfo.InvariantCulture),
                    DailyLow = decimal.Parse(currencyData.Low, CultureInfo.InvariantCulture),
                    PctChange = pctChange,
                    DecimalPlaces = item.DecimalPlaces,
                    LastUpdated = lastUpdated,
                });
            }

            cache.Set(CacheKey, quotes, TimeSpan.FromMinutes(15));
            return (quotes, null);
        }
        catch (Exception exception)
        {
            var techDetails = $"{exception.GetType().Name}: {exception.Message}";
            cache.Set(CacheKey, quotes, TimeSpan.FromMinutes(3));
            cache.Set(ErrorCacheKey, techDetails, TimeSpan.FromMinutes(3));
            return (quotes, techDetails);
        }
    }
}
