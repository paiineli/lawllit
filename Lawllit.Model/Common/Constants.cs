namespace Lawllit.Model.Common;

public static class Constants
{
    public const string DefaultLanguage = "pt-BR";
    public static readonly string[] ValidLanguages = [DefaultLanguage, "en-US"];
    public static readonly string[] ValidThemes = ["dark", "light", "high-contrast"];
    public static readonly string[] ValidFontSizes = ["normal", "large", "xlarge"];
    public const string DefaultCurrency = "BRL";
    public static readonly string[] ValidCurrencies = [DefaultCurrency, "USD"];

    public const string ApiKeyHeader = "X-Api-Key";
    public const string ApiTokenClaim = "api_token";
}
