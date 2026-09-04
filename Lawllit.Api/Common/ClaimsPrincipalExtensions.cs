using System.Security.Claims;

namespace Lawllit.Api.Common;

public static class ClaimsPrincipalExtensions
{
    // O escopo por usuário sai sempre do token, nunca de parâmetro da rota,
    // senão bastaria trocar o id na URL para ler o dado de outra pessoa.
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirst(JwtSettings.SubjectClaim)?.Value;

        if (!Guid.TryParse(subject, out var userId))
            throw new InvalidOperationException("Token autenticado sem claim 'sub' válida.");

        return userId;
    }
}
