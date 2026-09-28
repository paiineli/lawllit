using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Jogos;

public sealed class BaroRepository(IHttpClientFactory httpClientFactory) : IBaroRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    public Task<Resultado<BaroModel>> Buscar(CancellationToken cancellationToken)
        => httpClient.Enviar<BaroModel>(HttpMethod.Get, "api/jogos/baro", corpo: null, cancellationToken);
}

#region Interfaces

public interface IBaroRepository
{
    // Com a fonte fora do ar a API devolve o último Baro bom, e a falha só vem se nunca houve um.
    Task<Resultado<BaroModel>> Buscar(CancellationToken cancellationToken);
}

#endregion
