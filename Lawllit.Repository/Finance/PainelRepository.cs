using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace Lawllit.Repository.Finance;

public sealed class PainelRepository(IHttpClientFactory httpClientFactory) : IPainelRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    public async Task<PainelModel> Montar(PeriodoPainelEnum periodo, int? mes, int? ano, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?> { ["periodo"] = periodo.ToString() };

        if (mes.HasValue) query["mes"] = mes.Value.ToString();
        if (ano.HasValue) query["ano"] = ano.Value.ToString();

        var url = QueryHelpers.AddQueryString("api/financas/painel", query);

        return await httpClient.BuscarOuNulo<PainelModel>(url, cancellationToken)
            ?? throw new InvalidOperationException("A API não devolveu o painel do período.");
    }
}

#region Interfaces

public interface IPainelRepository
{
    // No modo mês, mês e ano nulos caem no mês corrente. No modo ano, o mês é ignorado.
    Task<PainelModel> Montar(PeriodoPainelEnum periodo, int? mes, int? ano, CancellationToken cancellationToken);
}

#endregion
