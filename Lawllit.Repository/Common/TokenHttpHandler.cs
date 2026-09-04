using Lawllit.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace Lawllit.Repository.Common;

// Injeta a chave compartilhada em toda chamada e o Bearer do usuário quando existe sessão.
// O JWT vem do cookie de autenticação, então não precisa de cache nem de reemissão aqui.
public sealed class TokenHttpHandler(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : DelegatingHandler
{
    // A ausência da chave já derruba a aplicação na subida, pelo DependencyContainer,
    // então aqui ela é sempre válida e não precisa de checagem por requisição.
    private readonly string apiKey = configuration["Api:Key"]!;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove(Constants.ApiKeyHeader);
        request.Headers.Add(Constants.ApiKeyHeader, apiKey);

        var apiToken = httpContextAccessor.HttpContext?.User.FindFirst(Constants.ApiTokenClaim)?.Value;

        if (!string.IsNullOrEmpty(apiToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        return base.SendAsync(request, cancellationToken);
    }
}
