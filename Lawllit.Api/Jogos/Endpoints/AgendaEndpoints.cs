using Lawllit.Api.Common;
using Lawllit.Api.Jogos.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Jogos.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Jogos.Endpoints;

public static class AgendaEndpoints
{
    public static RouteGroupBuilder MapAgenda(this RouteGroupBuilder builder)
    {
        builder.MapGet("/{jogo}", Listar)
            .WithSummary("Retorna as próximas partidas e os resultados de um jogo, lidos da Liquipedia em segundo plano")
            .WithTags("Jogos");

        return builder;
    }

    private static Results<Ok<AgendaModel>, BadRequest<ErroApiModel>> Listar(
        JogoEnum jogo,
        IAgendaService agendaService)
        => agendaService.Listar(jogo).ParaHttp();
}
