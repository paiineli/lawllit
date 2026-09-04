using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class AuthREP : IAuthREP
{
    #region DI

    private readonly HttpClient httpClient;

    public AuthREP(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(ApiClient.HttpClientName);
    }

    #endregion

    #region Methods

    public Task<Result<AuthResultMOD>> LoginAsync(LoginMOD login, CancellationToken cancellationToken)
        => httpClient.SendAsync<AuthResultMOD>(HttpMethod.Post, "api/auth/login", login, cancellationToken);

    public Task<Result> RegisterAsync(RegisterMOD register, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Post, "api/auth/register", register, cancellationToken);

    public Task<Result<AuthResultMOD>> ConfirmEmailAsync(string token, CancellationToken cancellationToken)
        => httpClient.SendAsync<AuthResultMOD>(HttpMethod.Get, $"api/auth/confirm-email/{Uri.EscapeDataString(token)}", body: null, cancellationToken);

    public Task<bool> IsPasswordResetTokenValidAsync(string token, CancellationToken cancellationToken)
        => httpClient.ExistsAsync($"api/auth/reset-token/{Uri.EscapeDataString(token)}", cancellationToken);

    public Task<Result> ForgotPasswordAsync(ForgotPasswordMOD forgotPassword, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Post, "api/auth/forgot-password", forgotPassword, cancellationToken);

    public Task<Result> ResetPasswordAsync(ResetPasswordMOD resetPassword, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Post, "api/auth/reset-password", resetPassword, cancellationToken);

    public Task<Result<AuthResultMOD>> LoginWithGoogleAsync(GoogleUserMOD googleUser, CancellationToken cancellationToken)
        => httpClient.SendAsync<AuthResultMOD>(HttpMethod.Post, "api/auth/google", googleUser, cancellationToken);

    #endregion
}

#region Interfaces

public interface IAuthREP
{
    /// <summary>
    /// Valida e-mail e senha na API e devolve o usuário autenticado com o token de acesso.
    /// </summary>
    Task<Result<AuthResultMOD>> LoginAsync(LoginMOD login, CancellationToken cancellationToken);

    /// <summary>
    /// Cria a conta e pede o disparo do e-mail de confirmação, usando o molde de URL informado.
    /// </summary>
    Task<Result> RegisterAsync(RegisterMOD register, CancellationToken cancellationToken);

    /// <summary>
    /// Confirma o e-mail pelo token recebido no link e já devolve o usuário autenticado.
    /// </summary>
    Task<Result<AuthResultMOD>> ConfirmEmailAsync(string token, CancellationToken cancellationToken);

    /// <summary>
    /// Informa se o token de redefinição de senha ainda é válido, antes de abrir o formulário.
    /// </summary>
    Task<bool> IsPasswordResetTokenValidAsync(string token, CancellationToken cancellationToken);

    /// <summary>
    /// Solicita o envio do e-mail de recuperação de senha.
    /// </summary>
    Task<Result> ForgotPasswordAsync(ForgotPasswordMOD forgotPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Troca a senha a partir do token de redefinição.
    /// </summary>
    Task<Result> ResetPasswordAsync(ResetPasswordMOD resetPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Autentica pelo Google, criando a conta ou vinculando a conta existente ao GoogleId.
    /// </summary>
    Task<Result<AuthResultMOD>> LoginWithGoogleAsync(GoogleUserMOD googleUser, CancellationToken cancellationToken);
}

#endregion
