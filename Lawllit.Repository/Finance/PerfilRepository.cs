using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class PerfilRepository(IHttpClientFactory httpClientFactory) : IPerfilRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    private const string Rota = "api/financas/perfil";

    public Task<PerfilModel?> Consultar(CancellationToken cancellationToken)
        => httpClient.BuscarOuNulo<PerfilModel>(Rota, cancellationToken);

    public Task<Resultado<UsuarioModel>> AlterarNome(AlterarNomeModel alterar, CancellationToken cancellationToken)
        => httpClient.Enviar<UsuarioModel>(HttpMethod.Patch, $"{Rota}/nome", alterar, cancellationToken);

    public Task<Resultado<UsuarioModel>> AlterarEmail(AlterarEmailModel alterar, CancellationToken cancellationToken)
        => httpClient.Enviar<UsuarioModel>(HttpMethod.Patch, $"{Rota}/email", alterar, cancellationToken);

    public Task<Resultado> AlterarSenha(AlterarSenhaModel alterar, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Patch, $"{Rota}/senha", alterar, cancellationToken);

    public Task<Resultado<UsuarioModel>> SalvarPreferencia(PreferenciaModel preferencia, CancellationToken cancellationToken)
        => httpClient.Enviar<UsuarioModel>(HttpMethod.Patch, $"{Rota}/preferencia", preferencia, cancellationToken);

    public Task<Resultado> ExcluirConta(ExcluirContaModel excluir, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Delete, Rota, excluir, cancellationToken);
}

#region Interfaces

// Quem devolve UsuarioModel devolve porque o Site reemite o cookie com o dado alterado.
public interface IPerfilRepository
{
    Task<PerfilModel?> Consultar(CancellationToken cancellationToken);

    Task<Resultado<UsuarioModel>> AlterarNome(AlterarNomeModel alterar, CancellationToken cancellationToken);

    Task<Resultado<UsuarioModel>> AlterarEmail(AlterarEmailModel alterar, CancellationToken cancellationToken);

    Task<Resultado> AlterarSenha(AlterarSenhaModel alterar, CancellationToken cancellationToken);

    Task<Resultado<UsuarioModel>> SalvarPreferencia(PreferenciaModel preferencia, CancellationToken cancellationToken);

    // Apaga de verdade, sem SN_ATIVO, porque a LGPD dá direito à eliminação.
    Task<Resultado> ExcluirConta(ExcluirContaModel excluir, CancellationToken cancellationToken);
}

#endregion
