using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class PainelEndpoints
{
    public static RouteGroupBuilder MapPainel(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", Montar)
            .WithSummary("Monta o painel do mês ou do ano, com totais, tendência, referência histórica e ranking")
            .WithTags("Painel");

        return builder;
    }

    private static async Task<Ok<PainelModel>> Montar(
        ClaimsPrincipal usuario,
        IPainelService servico,
        CancellationToken cancellationToken,
        PeriodoPainelEnum periodo = PeriodoPainelEnum.MES,
        int? mes = null,
        int? ano = null)
        => TypedResults.Ok(await servico.Montar(usuario.ObterCodigoUsuario(), periodo, mes, ano, cancellationToken));
}
