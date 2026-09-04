using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lawllit.Site.Areas.Finance.Controllers;

public abstract class BaseController : Controller
{
    // O token da API viaja dentro do cookie de sessão. Quem o injeta nas chamadas
    // é o TokenHttpHandler, o controller nunca toca no header.
    protected async Task SignInAsync(UserMOD user, string apiToken)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,  user.Id.ToString()),
            new(ClaimTypes.Name,            user.Name),
            new(ClaimTypes.Email,           user.Email),
            new("theme",                    user.Theme),
            new("font_size",                user.FontSize),
            new("language",                 user.Language),
            new("currency",                 user.Currency),
            new("is_onboarding_completed",  user.IsOnboardingCompleted.ToString().ToLower()),
            new(Constants.ApiTokenClaim,    apiToken),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    // Reemite o cookie depois de alterar dado do usuário, reaproveitando o token vigente,
    // já que o token só carrega o Id e continua válido.
    protected Task RefreshSignInAsync(UserMOD user)
        => SignInAsync(user, User.FindFirst(Constants.ApiTokenClaim)?.Value ?? string.Empty);
}
