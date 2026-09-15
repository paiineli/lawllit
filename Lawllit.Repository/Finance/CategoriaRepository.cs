using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace Lawllit.Repository.Finance;

public sealed class CategoriaRepository(IHttpClientFactory httpClientFactory) : ICategoriaRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    private const string Rota = "api/financas/categorias";

    public async Task<List<CategoriaModel>> Listar(CategoriaFiltroModel filtro, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>();

        if (filtro.Tipo.HasValue) query["tipo"] = filtro.Tipo.Value.ToString();
        if (!string.IsNullOrWhiteSpace(filtro.Busca)) query["busca"] = filtro.Busca;

        var url = QueryHelpers.AddQueryString(Rota, query);

        return await httpClient.BuscarOuNulo<List<CategoriaModel>>(url, cancellationToken) ?? [];
    }

    public Task<CategoriaModel?> BuscarPorCodigo(Guid codigo, CancellationToken cancellationToken)
        => httpClient.BuscarOuNulo<CategoriaModel>($"{Rota}/{codigo}", cancellationToken);

    public Task<Resultado> Criar(CategoriaSalvarModel categoria, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Post, Rota, categoria, cancellationToken);

    public Task<Resultado> Alterar(CategoriaSalvarModel categoria, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Patch, Rota, categoria, cancellationToken);

    public Task<Resultado> Inativar(Guid codigo, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Delete, $"{Rota}/{codigo}", corpo: null, cancellationToken);
}

#region Interfaces

public interface ICategoriaRepository
{
    Task<List<CategoriaModel>> Listar(CategoriaFiltroModel filtro, CancellationToken cancellationToken);

    Task<CategoriaModel?> BuscarPorCodigo(Guid codigo, CancellationToken cancellationToken);

    // Falha quando já existe outra categoria com o mesmo nome e tipo.
    Task<Resultado> Criar(CategoriaSalvarModel categoria, CancellationToken cancellationToken);

    Task<Resultado> Alterar(CategoriaSalvarModel categoria, CancellationToken cancellationToken);

    // As transações que apontam para ela continuam intactas, exibindo o nome no histórico.
    Task<Resultado> Inativar(Guid codigo, CancellationToken cancellationToken);
}

#endregion
