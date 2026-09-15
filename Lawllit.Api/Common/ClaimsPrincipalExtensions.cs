using System.Security.Claims;

namespace Lawllit.Api.Common;

public static class ClaimsPrincipalExtensions
{
    // Sai do token e nunca da rota, senão bastava trocar o código na URL para ler dado alheio.
    public static Guid ObterCodigoUsuario(this ClaimsPrincipal principal)
    {
        var assunto = principal.FindFirst(ConfiguracaoJwt.ClaimAssunto)?.Value;

        if (!Guid.TryParse(assunto, out var cdUsuario))
            throw new InvalidOperationException("Token autenticado sem claim 'sub' válida.");

        return cdUsuario;
    }
}
