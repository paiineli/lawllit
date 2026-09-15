using Lawllit.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace Lawllit.Repository.Common;

// O JWT vem do cookie de autenticação, então não precisa de cache nem de reemissão aqui.
public sealed class TokenHttpHandler(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : DelegatingHandler
{
    // A falta da chave já derruba a subida, no DependencyContainer, então aqui ela é sempre válida.
    private readonly string chaveApi = configuration["Api:Key"]!;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove(Constantes.CabecalhoChaveApi);
        request.Headers.Add(Constantes.CabecalhoChaveApi, chaveApi);

        var token = httpContextAccessor.HttpContext?.User.FindFirst(Constantes.ClaimTokenApi)?.Value;

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
