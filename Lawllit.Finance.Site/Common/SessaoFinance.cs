using Lawllit.Model.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using System.Security.Claims;

namespace Lawllit.Finance.Site.Common;

public static class SessaoFinance
{
    public static DateTimeOffset? ExpiracaoDoToken(ClaimsPrincipal usuario)
        => DateTimeOffset.TryParse(usuario.FindFirst(Constantes.ClaimTokenApiExpira)?.Value,
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var expira)
            ? expira
            : null;

    // Cookie antigo, sem a validade do token, ou com o token já vencido, vira login de novo.
    public static async Task Validar(CookieValidatePrincipalContext contexto)
    {
        var expira = contexto.Principal is null ? null : ExpiracaoDoToken(contexto.Principal);
        if (expira > DateTimeOffset.UtcNow)
            return;

        contexto.RejectPrincipal();
        await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
