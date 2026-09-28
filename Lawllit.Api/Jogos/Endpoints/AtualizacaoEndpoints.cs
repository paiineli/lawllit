using Lawllit.Api.Common;
using Lawllit.Api.Jogos.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Jogos.Endpoints;

public static class AtualizacaoEndpoints
{
    public static RouteGroupBuilder MapAtualizacoes(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", Listar)
            .WithSummary("Retorna o patch mais recente de cada jogo, com cache em memória")
            .WithTags("Jogos");

        return builder;
    }

    private static async Task<Results<Ok<List<AtualizacaoModel>>, BadRequest<ErroApiModel>>> Listar(
        IAtualizacaoService atualizacaoService,
        CancellationToken cancellationToken)
        => (await atualizacaoService.Listar(cancellationToken)).ParaHttp();
}
