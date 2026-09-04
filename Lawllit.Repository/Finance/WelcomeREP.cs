using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class WelcomeREP : IWelcomeREP
{
    #region DI

    private readonly HttpClient httpClient;

    public WelcomeREP(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(ApiClient.HttpClientName);
    }

    #endregion

    #region Methods

    public Task<WelcomeMOD?> GetAsync(int step, CancellationToken cancellationToken)
        => httpClient.GetOrDefaultAsync<WelcomeMOD>($"api/welcome?step={step}", cancellationToken);

    public Task<Result<UserMOD>> CompleteAsync(CancellationToken cancellationToken)
        => httpClient.SendAsync<UserMOD>(HttpMethod.Post, "api/welcome/complete", body: null, cancellationToken);

    #endregion
}

#region Interfaces

public interface IWelcomeREP
{
    /// <summary>
    /// Retorna o passo corrente do onboarding e as preferências já escolhidas pelo usuário.
    /// </summary>
    Task<WelcomeMOD?> GetAsync(int step, CancellationToken cancellationToken);

    /// <summary>
    /// Marca o onboarding como concluído e devolve o usuário atualizado.
    /// </summary>
    Task<Result<UserMOD>> CompleteAsync(CancellationToken cancellationToken);
}

#endregion
