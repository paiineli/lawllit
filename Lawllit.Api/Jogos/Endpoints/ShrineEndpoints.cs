using Lawllit.Api.Common;
using Lawllit.Api.Jogos.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Jogos.Endpoints;

public static class ShrineEndpoints
{
    public static RouteGroupBuilder MapShrine(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", Buscar)
            .WithSummary("Retorna as perks do Shrine of Secrets da semana, com cache em memória")
            .WithTags("Jogos");

        return builder;
    }

    private static async Task<Results<Ok<ShrineModel>, BadRequest<ErroApiModel>>> Buscar(
        IShrineService shrineService,
        CancellationToken cancellationToken)
        => (await shrineService.Buscar(cancellationToken)).ParaHttp();
}
