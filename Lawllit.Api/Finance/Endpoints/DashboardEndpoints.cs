using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Finance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboard(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", BuildAsync)
            .WithSummary("Monta o painel do mês com totais, tendência e ranking de despesas")
            .WithTags("Dashboard");

        return builder;
    }

    private static async Task<Ok<DashboardMOD>> BuildAsync(
        ClaimsPrincipal user,
        IDashboardService dashboardService,
        CancellationToken cancellationToken,
        int? month = null,
        int? year = null)
        => TypedResults.Ok(await dashboardService.BuildAsync(user.GetUserId(), month, year, cancellationToken));
}
