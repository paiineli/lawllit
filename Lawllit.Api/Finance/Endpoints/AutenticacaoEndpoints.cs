using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Finance.Endpoints;

public static class AutenticacaoEndpoints
{
    public static RouteGroupBuilder MapAutenticacao(this RouteGroupBuilder builder)
    {
        builder.MapPost("/entrar", Entrar)
            .WithSummary("Valida e-mail e senha e devolve o usuário com o token de acesso")
            .WithTags("Autenticação");

        builder.MapPost("/cadastrar", Cadastrar)
            .WithSummary("Cria a conta com as categorias padrão e dispara o e-mail de confirmação")
            .WithTags("Autenticação");

        builder.MapGet("/confirmar-email/{token}", ConfirmarEmail)
            .WithSummary("Confirma o e-mail pelo token e já autentica o usuário")
            .WithTags("Autenticação");

        builder.MapPost("/esqueci-senha", EsqueciSenha)
            .WithSummary("Gera o token de redefinição e envia o e-mail de recuperação")
            .WithTags("Autenticação");

        builder.MapGet("/token-senha/{token}", ValidarTokenSenha)
            .WithSummary("Informa se o token de redefinição ainda é válido")
            .WithTags("Autenticação");

        builder.MapPost("/redefinir-senha", RedefinirSenha)
            .WithSummary("Troca a senha a partir do token de redefinição")
            .WithTags("Autenticação");

        return builder;
    }

    private static async Task<Results<Ok<AutenticacaoModel>, BadRequest<ErroApiModel>>> Entrar(
        LoginModel login,
        IAutenticacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.Entrar(login, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> Cadastrar(
        CadastroModel cadastro,
        IAutenticacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.Cadastrar(cadastro, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok<AutenticacaoModel>, BadRequest<ErroApiModel>>> ConfirmarEmail(
        string token,
        IAutenticacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.ConfirmarEmail(token, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> EsqueciSenha(
        EsqueciSenhaModel esqueciSenha,
        IAutenticacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.EsqueciSenha(esqueciSenha, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok, NotFound>> ValidarTokenSenha(
        string token,
        IAutenticacaoService servico,
        CancellationToken cancellationToken)
        => await servico.TokenSenhaValido(token, cancellationToken)
            ? TypedResults.Ok()
            : TypedResults.NotFound();

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> RedefinirSenha(
        RedefinirSenhaModel redefinir,
        IAutenticacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.RedefinirSenha(redefinir, cancellationToken)).ParaHttp();
}
