using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class AutenticacaoRepository(IHttpClientFactory httpClientFactory) : IAutenticacaoRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    private const string Rota = "api/financas/autenticacao";

    public Task<Resultado<AutenticacaoModel>> Entrar(LoginModel login, CancellationToken cancellationToken)
        => httpClient.Enviar<AutenticacaoModel>(HttpMethod.Post, $"{Rota}/entrar", login, cancellationToken);

    public Task<Resultado> Cadastrar(CadastroModel cadastro, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Post, $"{Rota}/cadastrar", cadastro, cancellationToken);

    public Task<Resultado<AutenticacaoModel>> ConfirmarEmail(string token, CancellationToken cancellationToken)
        => httpClient.Enviar<AutenticacaoModel>(HttpMethod.Get, $"{Rota}/confirmar-email/{Uri.EscapeDataString(token)}", corpo: null, cancellationToken);

    public Task<bool> TokenSenhaValido(string token, CancellationToken cancellationToken)
        => httpClient.Existe($"{Rota}/token-senha/{Uri.EscapeDataString(token)}", cancellationToken);

    public Task<Resultado> EsqueciSenha(EsqueciSenhaModel esqueciSenha, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Post, $"{Rota}/esqueci-senha", esqueciSenha, cancellationToken);

    public Task<Resultado> RedefinirSenha(RedefinirSenhaModel redefinir, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Post, $"{Rota}/redefinir-senha", redefinir, cancellationToken);
}

#region Interfaces

public interface IAutenticacaoRepository
{
    Task<Resultado<AutenticacaoModel>> Entrar(LoginModel login, CancellationToken cancellationToken);

    // Cria a conta já com as categorias padrão e dispara o e-mail de confirmação.
    Task<Resultado> Cadastrar(CadastroModel cadastro, CancellationToken cancellationToken);

    Task<Resultado<AutenticacaoModel>> ConfirmarEmail(string token, CancellationToken cancellationToken);

    Task<bool> TokenSenhaValido(string token, CancellationToken cancellationToken);

    Task<Resultado> EsqueciSenha(EsqueciSenhaModel esqueciSenha, CancellationToken cancellationToken);

    Task<Resultado> RedefinirSenha(RedefinirSenhaModel redefinir, CancellationToken cancellationToken);
}

#endregion
