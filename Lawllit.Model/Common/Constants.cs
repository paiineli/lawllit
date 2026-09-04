namespace Lawllit.Model.Common;

public static class Constants
{
    public const string DefaultLanguage = "pt-BR";
    public static readonly string[] ValidLanguages = [DefaultLanguage, "en-US"];
    public static readonly string[] ValidThemes = ["dark", "light", "high-contrast"];
    public static readonly string[] ValidFontSizes = ["normal", "large", "xlarge"];
    // O app é em real. A preferência de moeda só trocava a formatação e não convertia
    // valor, então mostrar dólar dava um número que não existe. Real é a única opção.
    public const string DefaultCurrency = "BRL";
    public static readonly string[] ValidCurrencies = [DefaultCurrency];

    public const string ApiKeyHeader = "X-Api-Key";
    public const string ApiTokenClaim = "api_token";
}
