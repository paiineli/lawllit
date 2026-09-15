using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lawllit.Finance.Site.Controllers;

public abstract class BaseController : Controller
{
    // O token viaja no cookie e quem o injeta é o TokenHttpHandler, nunca o controller.
    protected async Task Autenticar(UsuarioModel usuario, string tokenApi)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.CdUsuario.ToString()),
            new(ClaimTypes.Name,           usuario.NmUsuario),
            new(ClaimTypes.Email,          usuario.TxEmail),
            new("tema",                    usuario.TxTema),
            new("tamanho_fonte",           usuario.TxTamanhoFonte),
            new(Constantes.ClaimTokenApi,  tokenApi),
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade),
            new AuthenticationProperties { IsPersistent = true });
    }

    // Reaproveita o token vigente: ele só carrega o código do usuário e continua válido.
    protected Task Reautenticar(UsuarioModel usuario)
        => Autenticar(usuario, User.FindFirst(Constantes.ClaimTokenApi)?.Value ?? string.Empty);
}
