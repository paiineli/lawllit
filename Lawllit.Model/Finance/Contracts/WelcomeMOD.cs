using Lawllit.Model.Common;

namespace Lawllit.Model.Finance.Contracts;

public sealed class WelcomeMOD
{
    public int CurrentStep { get; set; } = 1;
    public string UserName { get; set; } = string.Empty;
    public string Language { get; set; } = Constants.DefaultLanguage;
    public string Currency { get; set; } = Constants.DefaultCurrency;
    public string Theme { get; set; } = "dark";
    public string FontSize { get; set; } = "normal";
}
