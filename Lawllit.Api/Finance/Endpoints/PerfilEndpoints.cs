using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class PerfilEndpoints
{
    public static RouteGroupBuilder MapPerfil(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", Consultar)
            .WithSummary("Retorna os dados de perfil e as preferências do usuário")
            .WithTags("Perfil");

        builder.MapPatch("/nome", AlterarNome)
            .WithSummary("Altera o nome de exibição")
            .WithTags("Perfil");

        builder.MapPatch("/email", AlterarEmail)
            .WithSummary("Altera o e-mail, confirmando a senha atual")
            .WithTags("Perfil");

        builder.MapPatch("/senha", AlterarSenha)
            .WithSummary("Troca a senha validando a senha atual")
            .WithTags("Perfil");

        builder.MapPatch("/preferencia", SalvarPreferencia)
            .WithSummary("Salva uma preferência de tema ou de tamanho de fonte")
            .WithTags("Perfil");

        builder.MapDelete("/", ExcluirConta)
            .WithSummary("Exclui a conta e todo o dado vinculado, em definitivo")
            .WithTags("Perfil");

        return builder;
    }

    private static async Task<Results<Ok<PerfilModel>, NotFound>> Consultar(
        ClaimsPrincipal usuario,
        IPerfilService servico,
        CancellationToken cancellationToken)
    {
        var perfil = await servico.Consultar(usuario.ObterCodigoUsuario(), cancellationToken);

        return perfil is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(perfil);
    }

    private static async Task<Results<Ok<UsuarioModel>, BadRequest<ErroApiModel>>> AlterarNome(
        AlterarNomeModel alterar,
        ClaimsPrincipal usuario,
        IPerfilService servico,
        CancellationToken cancellationToken)
        => (await servico.AlterarNome(usuario.ObterCodigoUsuario(), alterar, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok<UsuarioModel>, BadRequest<ErroApiModel>>> AlterarEmail(
        AlterarEmailModel alterar,
        ClaimsPrincipal usuario,
        IPerfilService servico,
        CancellationToken cancellationToken)
        => (await servico.AlterarEmail(usuario.ObterCodigoUsuario(), alterar, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> AlterarSenha(
        AlterarSenhaModel alterar,
        ClaimsPrincipal usuario,
        IPerfilService servico,
        CancellationToken cancellationToken)
        => (await servico.AlterarSenha(usuario.ObterCodigoUsuario(), alterar, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok<UsuarioModel>, BadRequest<ErroApiModel>>> SalvarPreferencia(
        PreferenciaModel preferencia,
        ClaimsPrincipal usuario,
        IPerfilService servico,
        CancellationToken cancellationToken)
        => (await servico.SalvarPreferencia(usuario.ObterCodigoUsuario(), preferencia, cancellationToken)).ParaHttp();

    // O [FromBody] é obrigatório aqui. Minimal API não infere corpo em DELETE, e sem o
    // atributo a aplicação nem sobe.
    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> ExcluirConta(
        [FromBody] ExcluirContaModel excluir,
        ClaimsPrincipal usuario,
        IPerfilService servico,
        CancellationToken cancellationToken)
        => (await servico.ExcluirConta(usuario.ObterCodigoUsuario(), excluir, cancellationToken)).ParaHttp();
}
