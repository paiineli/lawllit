using Lawllit.Model.Common;
using Lawllit.Site.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lawllit.Site.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetLanguage(string language, string returnUrl)
    {
        if (!Constants.ValidLanguages.Contains(language))
            language = Constants.DefaultLanguage;

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture: language, uiCulture: language)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        // Usuário logado tem o idioma na claim, que ganha do cookie, então o cookie
        // sozinho não bastaria. Reemite a sessão com a claim nova.
        if (User.Identity?.IsAuthenticated == true)
        {
            var claims = User.Claims
                .Where(claim => claim.Type != "language")
                .Append(new Claim("language", language))
                .ToList();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IsPersistent = true });
        }

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "~/" : returnUrl);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewMOD(HttpContext.TraceIdentifier));
}
