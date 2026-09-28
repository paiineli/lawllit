using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Jogos.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Jogos;

public sealed class AgendaRepository(IHttpClientFactory httpClientFactory) : IAgendaRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    public Task<Resultado<AgendaModel>> Listar(JogoEnum jogo, CancellationToken cancellationToken)
        => httpClient.Enviar<AgendaModel>(HttpMethod.Get, $"api/jogos/agenda/{jogo}", corpo: null, cancellationToken);
}

#region Interfaces

public interface IAgendaRepository
{
    // Falha com a mensagem pronta logo depois da API subir, antes da primeira leitura da fonte.
    Task<Resultado<AgendaModel>> Listar(JogoEnum jogo, CancellationToken cancellationToken);
}

#endregion
