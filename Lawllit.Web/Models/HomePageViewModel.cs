namespace Lawllit.Web.Models;

public sealed record HomePageViewModel(
    IReadOnlyList<string> CoreTechStack,
    IReadOnlyList<string> ProjectTechStack,
    IReadOnlyList<string> TeamTechStack);
