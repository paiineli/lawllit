using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class WelcomeEndpoints
{
    public static RouteGroupBuilder MapWelcome(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetAsync)
            .WithSummary("Retorna o passo corrente do onboarding e as preferências já escolhidas")
            .WithTags("Welcome");

        builder.MapPost("/complete", CompleteAsync)
            .WithSummary("Marca o onboarding como concluído")
            .WithTags("Welcome");

        return builder;
    }

    private static async Task<Results<Ok<WelcomeMOD>, NotFound>> GetAsync(
        ClaimsPrincipal user,
        IWelcomeService welcomeService,
        CancellationToken cancellationToken,
        int step = 1)
    {
        var welcome = await welcomeService.GetAsync(user.GetUserId(), step, cancellationToken);

        return welcome is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(welcome);
    }

    private static async Task<Results<Ok<UserMOD>, BadRequest<ApiErrorMOD>>> CompleteAsync(
        ClaimsPrincipal user,
        IWelcomeService welcomeService,
        CancellationToken cancellationToken)
        => (await welcomeService.CompleteAsync(user.GetUserId(), cancellationToken)).ToHttpResult();
}
