using Lawllit.Web.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Web.Controllers;

public class HomeController : Controller
{
    private static readonly string[] CoreTechStack =
    [
        "C#",
        ".NET",
        "ASP.NET Core",
        "Entity Framework Core",
        "PostgreSQL"
    ];

    private static readonly string[] ProjectTechStack =
    [
        "C#",
        ".NET",
        "ASP.NET Core MVC",
        "Entity Framework Core",
        "PostgreSQL",
        "Google OAuth 2.0",
        "Bootstrap"
    ];

    private static readonly string[] TeamTechStack =
    [
        "C#",
        ".NET",
        "ASP.NET Core",
        "Entity Framework Core",
        "PostgreSQL",
        "Oracle"
    ];

    public IActionResult Index()
    {
        var viewModel = new HomePageViewModel(CoreTechStack, ProjectTechStack, TeamTechStack);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string language, string returnUrl)
    {
        if (!Constants.ValidLanguages.Contains(language))
            language = Constants.DefaultLanguage;

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture: language, uiCulture: language)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel(HttpContext.TraceIdentifier));
    }
}
