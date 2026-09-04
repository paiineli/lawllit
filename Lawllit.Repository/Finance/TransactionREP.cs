using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class TransactionREP : ITransactionREP
{
    #region DI

    private readonly HttpClient httpClient;

    public TransactionREP(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(ApiClient.HttpClientName);
    }

    #endregion

    #region Methods

    public async Task<TransactionPageMOD> GetPageAsync(TransactionFilterMOD filter, CancellationToken cancellationToken)
    {
        var result = await httpClient.SendAsync<TransactionPageMOD>(HttpMethod.Post, "api/transactions/paged", filter, cancellationToken);
        return result.Value!;
    }

    public async Task<List<TransactionMOD>> GetAllFilteredAsync(TransactionFilterMOD filter, CancellationToken cancellationToken)
    {
        var result = await httpClient.SendAsync<List<TransactionMOD>>(HttpMethod.Post, "api/transactions/export", filter, cancellationToken);
        return result.Value!;
    }

    public Task<TransactionMOD?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => httpClient.GetOrDefaultAsync<TransactionMOD>($"api/transactions/{id}", cancellationToken);

    public Task<Result> CreateAsync(TransactionSaveMOD transaction, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Post, "api/transactions", transaction, cancellationToken);

    public Task<Result> EditAsync(TransactionSaveMOD transaction, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Patch, "api/transactions", transaction, cancellationToken);

    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
        => httpClient.SendAsync(HttpMethod.Delete, $"api/transactions/{id}", body: null, cancellationToken);

    public async Task<int> ImportRecurringAsync(ImportRecurringMOD import, CancellationToken cancellationToken)
    {
        var result = await httpClient.SendAsync<ImportRecurringResultMOD>(HttpMethod.Post, "api/transactions/import-recurring", import, cancellationToken);
        return result.Value!.ImportedCount;
    }

    #endregion
}

#region Interfaces

public interface ITransactionREP
{
    /// <summary>
    /// Retorna a página de transações do filtro informado, junto dos totais do mês e da
    /// quantidade de recorrentes pendentes de importação.
    /// </summary>
    Task<TransactionPageMOD> GetPageAsync(TransactionFilterMOD filter, CancellationToken cancellationToken);

    /// <summary>
    /// Retorna todas as transações do filtro, sem paginação, para gerar a exportação.
    /// </summary>
    Task<List<TransactionMOD>> GetAllFilteredAsync(TransactionFilterMOD filter, CancellationToken cancellationToken);

    /// <summary>
    /// Retorna uma transação do usuário autenticado, ou nulo quando não existe.
    /// </summary>
    Task<TransactionMOD?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Cria uma transação. Falha quando a categoria informada não pertence ao usuário.
    /// </summary>
    Task<Result> CreateAsync(TransactionSaveMOD transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Altera uma transação existente.
    /// </summary>
    Task<Result> EditAsync(TransactionSaveMOD transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Exclui uma transação do usuário autenticado.
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Copia as transações recorrentes do mês anterior para o mês informado e devolve
    /// quantas foram criadas.
    /// </summary>
    Task<int> ImportRecurringAsync(ImportRecurringMOD import, CancellationToken cancellationToken);
}

#endregion
