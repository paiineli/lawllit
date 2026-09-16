using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class CategoriaEndpoints
{
    public static RouteGroupBuilder MapCategorias(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", Listar)
            .WithSummary("Lista as categorias ativas do usuário, com filtro opcional de tipo e nome")
            .WithTags("Categorias");

        builder.MapGet("/{codigo:guid}", BuscarPorCodigo)
            .WithSummary("Retorna uma categoria do usuário")
            .WithTags("Categorias");

        builder.MapPost("/", Criar)
            .WithSummary("Cria uma categoria")
            .WithTags("Categorias");

        builder.MapPatch("/", Alterar)
            .WithSummary("Altera nome e tipo de uma categoria")
            .WithTags("Categorias");

        builder.MapDelete("/{codigo:guid}", Inativar)
            .WithSummary("Inativa uma categoria, preservando o histórico das transações dela")
            .WithTags("Categorias");

        return builder;
    }

    private static async Task<Ok<List<CategoriaModel>>> Listar(
        ClaimsPrincipal usuario,
        ICategoriaService servico,
        CancellationToken cancellationToken,
        TipoTransacaoEnum? tipo = null,
        string? busca = null)
    {
        var filtro = new CategoriaFiltroModel { Tipo = tipo, Busca = busca };
        return TypedResults.Ok(await servico.Listar(usuario.ObterCodigoUsuario(), filtro, cancellationToken));
    }

    private static async Task<Results<Ok<CategoriaModel>, NotFound>> BuscarPorCodigo(
        Guid codigo,
        ClaimsPrincipal usuario,
        ICategoriaService servico,
        CancellationToken cancellationToken)
    {
        var categoria = await servico.BuscarPorCodigo(usuario.ObterCodigoUsuario(), codigo, cancellationToken);

        return categoria is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(categoria);
    }

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> Criar(
        CategoriaSalvarModel categoria,
        ClaimsPrincipal usuario,
        ICategoriaService servico,
        CancellationToken cancellationToken)
        => (await servico.Criar(usuario.ObterCodigoUsuario(), categoria, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> Alterar(
        CategoriaSalvarModel categoria,
        ClaimsPrincipal usuario,
        ICategoriaService servico,
        CancellationToken cancellationToken)
        => (await servico.Alterar(usuario.ObterCodigoUsuario(), categoria, cancellationToken)).ParaHttp();

    private static async Task<Results<Ok, BadRequest<ErroApiModel>>> Inativar(
        Guid codigo,
        ClaimsPrincipal usuario,
        ICategoriaService servico,
        CancellationToken cancellationToken)
        => (await servico.Inativar(usuario.ObterCodigoUsuario(), codigo, cancellationToken)).ParaHttp();
}
