using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class QuoteREP : IQuoteREP
{
    #region DI

    private readonly HttpClient httpClient;

    public QuoteREP(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(ApiClient.HttpClientName);
    }

    #endregion

    #region Methods

    public Task<Result<List<QuoteMOD>>> GetQuotesAsync(CancellationToken cancellationToken)
        => httpClient.SendAsync<List<QuoteMOD>>(HttpMethod.Get, "api/quotes", body: null, cancellationToken);

    #endregion
}

#region Interfaces

public interface IQuoteREP
{
    /// <summary>
    /// Retorna as cotações do dia. Falha quando a fonte externa está indisponível,
    /// trazendo a chave da mensagem para o Site exibir.
    /// </summary>
    Task<Result<List<QuoteMOD>>> GetQuotesAsync(CancellationToken cancellationToken);
}

#endregion
