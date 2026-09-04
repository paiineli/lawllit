using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Finance.Endpoints;

public static class QuoteEndpoints
{
    public static RouteGroupBuilder MapQuotes(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetQuotesAsync)
            .WithSummary("Retorna as cotações do dia, com cache em memória")
            .WithTags("Quotes");

        return builder;
    }

    private static async Task<Results<Ok<List<QuoteMOD>>, BadRequest<ApiErrorMOD>>> GetQuotesAsync(
        IQuotesService quotesService,
        CancellationToken cancellationToken)
        => (await quotesService.GetQuotesAsync(cancellationToken)).ToHttpResult();
}
