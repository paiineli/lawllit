namespace Lawllit.Web.Areas.Finance.Models;

public class QuoteViewModel
{
    public string Label { get; set; } = string.Empty;
    public string FlagEmoji { get; set; } = string.Empty;
    public decimal BuyRate { get; set; }
    public decimal DailyHigh { get; set; }
    public decimal DailyLow { get; set; }
    public decimal PctChange { get; set; }
    public int DecimalPlaces { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class QuoteApiData
{
    [System.Text.Json.Serialization.JsonPropertyName("bid")]
    public string Bid { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("high")]
    public string High { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("low")]
    public string Low { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("pctChange")]
    public string PctChange { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}
