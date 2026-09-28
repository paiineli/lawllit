using Lawllit.Api.Common;
using Lawllit.Api.Jogos.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Jogos.Endpoints;

public static class BaroEndpoints
{
    public static RouteGroupBuilder MapBaro(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", Buscar)
            .WithSummary("Retorna a próxima visita do Baro Ki'Teer, ou a atual com os itens, com cache em memória")
            .WithTags("Jogos");

        return builder;
    }

    private static async Task<Results<Ok<BaroModel>, BadRequest<ErroApiModel>>> Buscar(
        IBaroService baroService,
        CancellationToken cancellationToken)
        => (await baroService.Buscar(cancellationToken)).ParaHttp();
}
