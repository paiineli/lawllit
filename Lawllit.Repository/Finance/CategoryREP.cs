using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace Lawllit.Repository.Finance;

public sealed class CategoryREP : ICategoryREP
{
    #region DI

    private readonly HttpClient httpClient;

    public CategoryREP(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(ApiClient.HttpClientName);
    }

    #endregion

    #region Methods

    public async Task<List<CategoryMOD>> GetFilteredAsync(CategoryFilterMOD filter, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>();

        if (filter.Type.HasValue) query["type"] = filter.Type.Value.ToString();
        if (!string.IsNullOrWhiteSpace(filter.Search)) query["search"] = filter.Search;

        var url = QueryHelpers.AddQueryString("api/categories", query);

        return await httpClient.GetOrDefaultAsync<List<CategoryMOD>>(url, cancellationToken) ?? [];
    }

    public Task<CategoryMOD?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => httpClient.GetOrDefaultAsync<CategoryMOD>($"api/categories/{id}", cancellationToken);

    public Task<Result> CreateAsync(CategorySaveMOD category, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Post, "api/categories", category, cancellationToken);

    public Task<Result> EditAsync(CategorySaveMOD category, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Patch, "api/categories", category, cancellationToken);

    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Delete, $"api/categories/{id}", body: null, cancellationToken);

    #endregion
}

#region Interfaces

public interface ICategoryREP
{
    /// <summary>
    /// Lista as categorias do usuário autenticado, aplicando o filtro de tipo e de nome na API.
    /// </summary>
    Task<List<CategoryMOD>> GetFilteredAsync(CategoryFilterMOD filter, CancellationToken cancellationToken);

    /// <summary>
    /// Retorna uma categoria do usuário autenticado, ou nulo quando não existe.
    /// </summary>
    Task<CategoryMOD?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Cria uma categoria. Falha quando já existe outra com o mesmo nome e tipo.
    /// </summary>
    Task<Result> CreateAsync(CategorySaveMOD category, CancellationToken cancellationToken);

    /// <summary>
    /// Altera nome e tipo de uma categoria existente.
    /// </summary>
    Task<Result> EditAsync(CategorySaveMOD category, CancellationToken cancellationToken);

    /// <summary>
    /// Exclui uma categoria. Falha quando existe transação vinculada a ela.
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

#endregion
