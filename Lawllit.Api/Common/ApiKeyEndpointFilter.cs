using Lawllit.Model.Common;
using System.Security.Cryptography;
using System.Text;

namespace Lawllit.Api.Common;

// A API não tem domínio público, só o Site alcança ela pela rede privada.
// A chave compartilhada é a segunda barreira, para nada mais dentro da rede
// conseguir falar com os endpoints anônimos de autenticação.
public sealed class ApiKeyEndpointFilter(IConfiguration configuration) : IEndpointFilter
{
    private readonly byte[] expectedApiKey = Encoding.UTF8.GetBytes(configuration["Api:Key"] ?? string.Empty);

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var receivedApiKey = context.HttpContext.Request.Headers[Constants.ApiKeyHeader].ToString();

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(receivedApiKey), expectedApiKey))
            return TypedResults.Unauthorized();

        return await next(context);
    }
}
