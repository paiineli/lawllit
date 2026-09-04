namespace Lawllit.Model.Finance.Contracts;

// LabelKey em vez do texto pronto, quem traduz é o Site com a cultura do usuário.
public sealed class QuoteMOD
{
    public string LabelKey { get; set; } = string.Empty;
    public string FlagEmoji { get; set; } = string.Empty;
    public decimal BuyRate { get; set; }
    public decimal DailyHigh { get; set; }
    public decimal DailyLow { get; set; }
    public decimal PctChange { get; set; }
    public int DecimalPlaces { get; set; }
    public DateTime LastUpdated { get; set; }
}
