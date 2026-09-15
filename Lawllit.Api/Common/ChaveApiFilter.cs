using Lawllit.Model.Common;
using System.Security.Cryptography;
using System.Text;

namespace Lawllit.Api.Common;

// Segunda barreira além do JWT, porque os endpoints de autenticação são anônimos.
public sealed class ChaveApiFilter(IConfiguration configuration) : IEndpointFilter
{
    private readonly byte[] chaveEsperada = Encoding.UTF8.GetBytes(configuration["Api:Key"] ?? string.Empty);

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var chaveRecebida = context.HttpContext.Request.Headers[Constantes.CabecalhoChaveApi].ToString();

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(chaveRecebida), chaveEsperada))
            return TypedResults.Unauthorized();

        return await next(context);
    }
}
