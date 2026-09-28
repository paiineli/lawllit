using Lawllit.Model.Common;
using Microsoft.Extensions.Caching.Memory;

namespace Lawllit.Api.Common;

public static class CacheExtensions
{
    private static readonly TimeSpan ValidadeFalha = TimeSpan.FromMinutes(3);

    // Com a fonte externa fora do ar, devolve o último dado bom em vez da mensagem de erro.
    // A falha fica pouco tempo no cache, para tentar de novo sem martelar a fonte.
    public static async Task<Resultado<TValor>> BuscarComReserva<TValor>(
        this IMemoryCache cache,
        string chave,
        TimeSpan validade,
        Func<CancellationToken, Task<Resultado<TValor>>> consultar,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(chave, out Resultado<TValor>? emCache) && emCache is not null)
            return emCache;

        var chaveReserva = $"{chave}:reserva";
        var resultado = await consultar(cancellationToken);

        if (resultado.Sucesso)
        {
            cache.Set(chave, resultado, validade);
            cache.Set(chaveReserva, resultado);
            return resultado;
        }

        var reserva = cache.Get<Resultado<TValor>>(chaveReserva) ?? resultado;
        cache.Set(chave, reserva, ValidadeFalha);

        return reserva;
    }
}
