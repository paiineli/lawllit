using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Lawllit.Web.Models;

namespace Lawllit.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

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
