using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Jogos;

public sealed class AtualizacaoRepository(IHttpClientFactory httpClientFactory) : IAtualizacaoRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    public Task<Resultado<List<AtualizacaoModel>>> Listar(CancellationToken cancellationToken)
        => httpClient.Enviar<List<AtualizacaoModel>>(HttpMethod.Get, "api/jogos/atualizacoes", corpo: null, cancellationToken);
}

#region Interfaces

public interface IAtualizacaoRepository
{
    // Um patch por jogo. Jogo cuja fonte falhou fica de fora da lista em vez de derrubar as outras.
    Task<Resultado<List<AtualizacaoModel>>> Listar(CancellationToken cancellationToken);
}

#endregion
