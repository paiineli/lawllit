using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Jogos;

public sealed class ShrineRepository(IHttpClientFactory httpClientFactory) : IShrineRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    public Task<Resultado<ShrineModel>> Buscar(CancellationToken cancellationToken)
        => httpClient.Enviar<ShrineModel>(HttpMethod.Get, "api/jogos/shrine", corpo: null, cancellationToken);
}

#region Interfaces

public interface IShrineRepository
{
    // Com a fonte fora do ar a API devolve o último Shrine bom, e a falha só vem se nunca houve um.
    Task<Resultado<ShrineModel>> Buscar(CancellationToken cancellationToken);
}

#endregion
