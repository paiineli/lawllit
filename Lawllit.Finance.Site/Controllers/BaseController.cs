using Lawllit.Finance.Site.Common;
using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contratos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace Lawllit.Finance.Site.Controllers;

public abstract class BaseController : Controller
{
    // O token viaja no cookie e quem o injeta é o TokenHttpHandler, nunca o controller.
    // O cookie vence junto com o token, senão sobrava sessão viva que a API já recusa.
    protected async Task Autenticar(UsuarioSessaoModel usuario, string tokenApi, DateTimeOffset tokenExpiraEm)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Codigo.ToString()),
            new(ClaimTypes.Name,           usuario.Nome),
            new(ClaimTypes.Email,          usuario.Email),
            new("tema",                    usuario.Tema),
            new("tamanho_fonte",           usuario.TamanhoFonte),
            new(Constantes.ClaimTokenApi,  tokenApi),
            new(Constantes.ClaimTokenApiExpira, tokenExpiraEm.ToString("O", CultureInfo.InvariantCulture)),
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = tokenExpiraEm, AllowRefresh = false });
    }

    // Reaproveita o token vigente e a validade dele: trocar o nome não pode esticar a sessão.
    protected Task Reautenticar(UsuarioSessaoModel usuario)
        => Autenticar(
            usuario,
            User.FindFirst(Constantes.ClaimTokenApi)?.Value ?? string.Empty,
            SessaoFinance.ExpiracaoDoToken(User) ?? DateTimeOffset.UtcNow);
}
