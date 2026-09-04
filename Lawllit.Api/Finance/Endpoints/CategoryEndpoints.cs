using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategories(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetFilteredAsync)
            .WithSummary("Lista as categorias do usuário, com filtro opcional de tipo e nome")
            .WithTags("Categories");

        builder.MapGet("/{id:guid}", GetByIdAsync)
            .WithSummary("Retorna uma categoria do usuário")
            .WithTags("Categories");

        builder.MapPost("/", CreateAsync)
            .WithSummary("Cria uma categoria")
            .WithTags("Categories");

        builder.MapPatch("/", EditAsync)
            .WithSummary("Altera nome e tipo de uma categoria")
            .WithTags("Categories");

        builder.MapDelete("/{id:guid}", DeleteAsync)
            .WithSummary("Exclui uma categoria sem transação vinculada")
            .WithTags("Categories");

        return builder;
    }

    private static async Task<Ok<List<CategoryMOD>>> GetFilteredAsync(
        ClaimsPrincipal user,
        ICategoryService categoryService,
        CancellationToken cancellationToken,
        TransactionTypeEnum? type = null,
        string? search = null)
    {
        var filter = new CategoryFilterMOD { Type = type, Search = search };
        return TypedResults.Ok(await categoryService.GetFilteredAsync(user.GetUserId(), filter, cancellationToken));
    }

    private static async Task<Results<Ok<CategoryMOD>, NotFound>> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        ICategoryService categoryService,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.GetByIdAsync(user.GetUserId(), id, cancellationToken);

        return category is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(category);
    }

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> CreateAsync(
        CategorySaveMOD category,
        ClaimsPrincipal user,
        ICategoryService categoryService,
        CancellationToken cancellationToken)
        => (await categoryService.CreateAsync(user.GetUserId(), category, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> EditAsync(
        CategorySaveMOD category,
        ClaimsPrincipal user,
        ICategoryService categoryService,
        CancellationToken cancellationToken)
        => (await categoryService.EditAsync(user.GetUserId(), category, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> DeleteAsync(
        Guid id,
        ClaimsPrincipal user,
        ICategoryService categoryService,
        CancellationToken cancellationToken)
        => (await categoryService.DeleteAsync(user.GetUserId(), id, cancellationToken)).ToHttpResult();
}
