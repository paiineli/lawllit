using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class TransacaoEndpoints
{
    public static RouteGroupBuilder MapTransacoes(this RouteGroupBuilder builder)
    {
        builder.MapPost("/paginacao", Listar)
            .WithSummary("Retorna a página de transações do mês com os totais do filtro aplicado")
            .WithTags("Transações");

        builder.MapPost("/exportacao", ListarTodas)
            .WithSummary("Retorna todas as transações do filtro, sem paginação, para exportação")
            .WithTags("Transações");

        builder.MapGet("/{codigo:guid}", BuscarPorCodigo)
            .WithSummary("Retorna uma transação do usuário")
            .WithTags("Transações");

        builder.MapPost("/", Criar)
            .WithSummary("Cria uma transação")
            .WithTags("Transações");

        builder.MapPatch("/", Alterar)
            .WithSummary("Altera uma transação")
            .WithTags("Transações");

        builder.MapDelete("/{codigo:guid}", Inativar)
            .WithSummary("Inativa uma transação")
            .WithTags("Transações");

        builder.MapPost("/importar-recorrentes", ImportarRecorrentes)
            .WithSummary("Copia as transações recorrentes do mês anterior para o mês informado")
            .WithTags("Transações");

        return builder;
    }

    private static async Task<Ok<TransacaoPaginaModel>> Listar(
        TransacaoFiltroModel filtro,
        ClaimsPrincipal usuario,
        ITransacaoService servico,
        CancellationToken cancellationToken)
        => TypedResults.Ok(await servico.Listar(usuario.ObterCodigoUsuario(), filtro, cancellationToken));

    private static async Task<Ok<List<TransacaoModel>>> ListarTodas(
        TransacaoFiltroModel filtro,
        ClaimsPrincipal usuario,
        ITransacaoService servico,
        CancellationToken cancellationToken)
        => TypedResults.Ok(await servico.ListarTodas(usuario.ObterCodigoUsuario(), filtro, cancellationToken));

    private static async Task<Results<Ok<TransacaoModel>, NotFound>> BuscarPorCodigo(
        Guid codigo,
        ClaimsPrincipal usuario,
        ITransacaoService servico,
        CancellationToken cancellationToken)
    {
        var transacao = await servico.BuscarPorCodigo(usuario.ObterCodigoUsuario(), codigo, cancellationToken);

        return transacao is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(transacao);
    }

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> Criar(
        TransacaoSalvarModel transacao,
        ClaimsPrincipal usuario,
        ITransacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.Criar(usuario.ObterCodigoUsuario(), transacao, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> Alterar(
        TransacaoSalvarModel transacao,
        ClaimsPrincipal usuario,
        ITransacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.Alterar(usuario.ObterCodigoUsuario(), transacao, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> Inativar(
        Guid codigo,
        ClaimsPrincipal usuario,
        ITransacaoService servico,
        CancellationToken cancellationToken)
        => (await servico.Inativar(usuario.ObterCodigoUsuario(), codigo, cancellationToken)).ParaHttp();

    private static async Task<Ok<ImportarRecorrentesResultadoModel>> ImportarRecorrentes(
        ImportarRecorrentesModel importar,
        ClaimsPrincipal usuario,
        ITransacaoService servico,
        CancellationToken cancellationToken)
        => TypedResults.Ok(await servico.ImportarRecorrentes(usuario.ObterCodigoUsuario(), importar, cancellationToken));
}
