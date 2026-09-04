using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class ProfileREP : IProfileREP
{
    #region DI

    private readonly HttpClient httpClient;

    public ProfileREP(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(ApiClient.HttpClientName);
    }

    #endregion

    #region Methods

    public Task<ProfileMOD?> GetAsync(CancellationToken cancellationToken)
        => httpClient.GetOrDefaultAsync<ProfileMOD>("api/profile", cancellationToken);

    public Task<Result<UserMOD>> EditNameAsync(EditNameMOD editName, CancellationToken cancellationToken)
        => httpClient.SendAsync<UserMOD>(HttpMethod.Patch, "api/profile/name", editName, cancellationToken);

    public Task<Result<UserMOD>> EditEmailAsync(EditEmailMOD editEmail, CancellationToken cancellationToken)
        => httpClient.SendAsync<UserMOD>(HttpMethod.Patch, "api/profile/email", editEmail, cancellationToken);

    public Task<Result> ChangePasswordAsync(ChangePasswordMOD changePassword, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Patch, "api/profile/password", changePassword, cancellationToken);

    public Task<Result<UserMOD>> SavePreferenceAsync(PreferenceMOD preference, CancellationToken cancellationToken)
        => httpClient.SendAsync<UserMOD>(HttpMethod.Patch, "api/profile/preference", preference, cancellationToken);

    public Task<Result> DeleteAccountAsync(DeleteAccountMOD deleteAccount, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Delete, "api/profile", deleteAccount, cancellationToken);

    #endregion
}

#region Interfaces

public interface IProfileREP
{
    /// <summary>
    /// Retorna os dados de perfil e as preferências do usuário autenticado.
    /// </summary>
    Task<ProfileMOD?> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Altera o nome de exibição e devolve o usuário atualizado, para o Site reemitir o cookie.
    /// </summary>
    Task<Result<UserMOD>> EditNameAsync(EditNameMOD editName, CancellationToken cancellationToken);

    /// <summary>
    /// Altera o e-mail, exigindo a senha atual quando a conta tem senha cadastrada.
    /// </summary>
    Task<Result<UserMOD>> EditEmailAsync(EditEmailMOD editEmail, CancellationToken cancellationToken);

    /// <summary>
    /// Troca a senha validando a senha atual.
    /// </summary>
    Task<Result> ChangePasswordAsync(ChangePasswordMOD changePassword, CancellationToken cancellationToken);

    /// <summary>
    /// Salva uma preferência de tema, tamanho de fonte, idioma ou moeda.
    /// </summary>
    Task<Result<UserMOD>> SavePreferenceAsync(PreferenceMOD preference, CancellationToken cancellationToken);

    /// <summary>
    /// Exclui a conta do usuário autenticado e todo o dado vinculado a ela.
    /// </summary>
    Task<Result> DeleteAccountAsync(DeleteAccountMOD deleteAccount, CancellationToken cancellationToken);
}

#endregion
