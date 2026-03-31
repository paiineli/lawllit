namespace Lawllit.Models.Finance.ViewModels;

public class WelcomeViewModel
{
    public int CurrentStep { get; set; } = 1;
    public string UserName { get; set; } = string.Empty;
    public string Language { get; set; } = "pt-BR";
    public string Currency { get; set; } = "BRL";
    public string Theme { get; set; } = "dark";
    public string FontSize { get; set; } = "normal";
}
