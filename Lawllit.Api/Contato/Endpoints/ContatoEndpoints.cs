using Lawllit.Api.Common;
using Lawllit.Api.Contato.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Contato.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Contato.Endpoints;

public static class ContatoEndpoints
{
    public static RouteGroupBuilder MapContato(this RouteGroupBuilder builder)
    {
        builder.MapPost("/", Enviar)
            .WithSummary("Guarda a mensagem do formulário de contato e avisa por e-mail")
            .WithTags("Contato");

        return builder;
    }

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> Enviar(
        EnviarMensagemModel requisicao,
        IContatoService contatoService,
        CancellationToken cancellationToken)
        => (await contatoService.Enviar(requisicao, cancellationToken)).ParaHttp();
}
