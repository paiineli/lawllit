using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class CotacaoRepository(IHttpClientFactory httpClientFactory) : ICotacaoRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    public Task<Resultado<List<CotacaoModel>>> Listar(CancellationToken cancellationToken)
        => httpClient.Enviar<List<CotacaoModel>>(HttpMethod.Get, "api/financas/cotacoes", corpo: null, cancellationToken);
}

#region Interfaces

public interface ICotacaoRepository
{
    // Falha com a mensagem pronta quando a fonte externa está indisponível.
    Task<Resultado<List<CotacaoModel>>> Listar(CancellationToken cancellationToken);
}

#endregion
