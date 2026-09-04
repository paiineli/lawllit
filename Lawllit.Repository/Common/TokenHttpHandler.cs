using Lawllit.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Lawllit.Repository.Common;

// Injeta a chave compartilhada em toda chamada e o Bearer do usuário quando existe sessão.
// O JWT vem do cookie de autenticação, então não precisa de cache nem de reemissão aqui.
public sealed class TokenHttpHandler(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove(Constants.ApiKeyHeader);
        request.Headers.Add(Constants.ApiKeyHeader, configuration["Api:Key"]
            ?? throw new InvalidOperationException("Api__Key não configurada no ambiente."));

        var apiToken = httpContextAccessor.HttpContext?.User.FindFirst(Constants.ApiTokenClaim)?.Value;

        if (!string.IsNullOrEmpty(apiToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        return base.SendAsync(request, cancellationToken);
    }
}
