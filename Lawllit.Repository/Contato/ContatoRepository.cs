using Lawllit.Model.Common;
using Lawllit.Model.Contato.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Contato;

public sealed class ContatoRepository(IHttpClientFactory httpClientFactory) : IContatoRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    public Task<Resultado> Enviar(EnviarMensagemModel mensagem, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Post, "api/contato", mensagem, cancellationToken);
}

#region Interfaces

public interface IContatoRepository
{
    // Guarda a mensagem e dispara o aviso por e-mail para a caixa do Lucas.
    Task<Resultado> Enviar(EnviarMensagemModel mensagem, CancellationToken cancellationToken);
}

#endregion
