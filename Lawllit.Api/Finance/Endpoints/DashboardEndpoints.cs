using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboard(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", BuildAsync)
            .WithSummary("Monta o painel do mês ou do ano, com totais, tendência, referência histórica e ranking")
            .WithTags("Dashboard");

        return builder;
    }

    private static async Task<Ok<DashboardMOD>> BuildAsync(
        ClaimsPrincipal user,
        IDashboardService dashboardService,
        CancellationToken cancellationToken,
        DashboardPeriodEnum period = DashboardPeriodEnum.MONTH,
        int? month = null,
        int? year = null)
        => TypedResults.Ok(await dashboardService.BuildAsync(user.GetUserId(), period, month, year, cancellationToken));
}
