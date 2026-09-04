using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class TransactionEndpoints
{
    public static RouteGroupBuilder MapTransactions(this RouteGroupBuilder builder)
    {
        builder.MapPost("/paged", GetPageAsync)
            .WithSummary("Retorna a página de transações do mês com os totais do filtro aplicado")
            .WithTags("Transactions");

        builder.MapGet("/{id:guid}", GetByIdAsync)
            .WithSummary("Retorna uma transação do usuário")
            .WithTags("Transactions");

        builder.MapPost("/", CreateAsync)
            .WithSummary("Cria uma transação")
            .WithTags("Transactions");

        builder.MapPatch("/", EditAsync)
            .WithSummary("Altera uma transação")
            .WithTags("Transactions");

        builder.MapDelete("/{id:guid}", DeleteAsync)
            .WithSummary("Exclui uma transação")
            .WithTags("Transactions");

        builder.MapPost("/import-recurring", ImportRecurringAsync)
            .WithSummary("Copia as transações recorrentes do mês anterior para o mês informado")
            .WithTags("Transactions");

        return builder;
    }

    private static async Task<Ok<TransactionPageMOD>> GetPageAsync(
        TransactionFilterMOD filter,
        ClaimsPrincipal user,
        ITransactionService transactionService,
        CancellationToken cancellationToken)
        => TypedResults.Ok(await transactionService.GetPageAsync(user.GetUserId(), filter, cancellationToken));

    private static async Task<Results<Ok<TransactionMOD>, NotFound>> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        ITransactionService transactionService,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionService.GetByIdAsync(user.GetUserId(), id, cancellationToken);

        return transaction is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(transaction);
    }

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> CreateAsync(
        TransactionSaveMOD transaction,
        ClaimsPrincipal user,
        ITransactionService transactionService,
        CancellationToken cancellationToken)
        => (await transactionService.CreateAsync(user.GetUserId(), transaction, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> EditAsync(
        TransactionSaveMOD transaction,
        ClaimsPrincipal user,
        ITransactionService transactionService,
        CancellationToken cancellationToken)
        => (await transactionService.EditAsync(user.GetUserId(), transaction, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> DeleteAsync(
        Guid id,
        ClaimsPrincipal user,
        ITransactionService transactionService,
        CancellationToken cancellationToken)
        => (await transactionService.DeleteAsync(user.GetUserId(), id, cancellationToken)).ToHttpResult();

    private static async Task<Ok<ImportRecurringResultMOD>> ImportRecurringAsync(
        ImportRecurringMOD import,
        ClaimsPrincipal user,
        ITransactionService transactionService,
        CancellationToken cancellationToken)
        => TypedResults.Ok(await transactionService.ImportRecurringAsync(user.GetUserId(), import, cancellationToken));
}
