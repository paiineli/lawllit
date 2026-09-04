using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace Lawllit.Repository.Finance;

public sealed class DashboardREP : IDashboardREP
{
    #region DI

    private readonly HttpClient httpClient;

    public DashboardREP(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(ApiClient.HttpClientName);
    }

    #endregion

    #region Methods

    public async Task<DashboardMOD> BuildAsync(DashboardPeriodEnum period, int? month, int? year, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?> { ["period"] = period.ToString() };

        if (month.HasValue) query["month"] = month.Value.ToString();
        if (year.HasValue) query["year"] = year.Value.ToString();

        var url = QueryHelpers.AddQueryString("api/dashboard", query);

        return await httpClient.GetOrDefaultAsync<DashboardMOD>(url, cancellationToken)
            ?? throw new InvalidOperationException("A API não devolveu o painel do período.");
    }

    #endregion
}

#region Interfaces

public interface IDashboardREP
{
    /// <summary>
    /// Monta o painel do período informado. No modo mês, mês e ano nulos caem no mês
    /// corrente. No modo ano, o mês é ignorado.
    /// </summary>
    Task<DashboardMOD> BuildAsync(DashboardPeriodEnum period, int? month, int? year, CancellationToken cancellationToken);
}

#endregion
