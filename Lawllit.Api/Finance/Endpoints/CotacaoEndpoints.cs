using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Finance.Endpoints;

public static class CotacaoEndpoints
{
    public static RouteGroupBuilder MapCotacoes(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", Listar)
            .WithSummary("Retorna as cotações do dia, com cache em memória")
            .WithTags("Cotações");

        return builder;
    }

    private static async Task<Results<Ok<List<CotacaoModel>>, BadRequest<ErroApiModel>>> Listar(
        ICotacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.Listar(cancellationToken)).ParaHttp();
}
