using Lawllit.Data.Finance.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lawllit.Web.Areas.Finance.Controllers;

public abstract class BaseController : Controller
{
    protected Guid GetUserId()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            throw new InvalidOperationException("Authenticated user has no valid ID claim.");
        return userId;
    }

    protected async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Name),
            new(ClaimTypes.Email,          user.Email),
            new("theme",                   user.Theme),
            new("font_size",               user.FontSize),
            new("language",                user.Language),
            new("currency",                user.Currency),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });
    }
}
