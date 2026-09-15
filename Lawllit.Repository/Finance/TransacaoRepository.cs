using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Common;

namespace Lawllit.Repository.Finance;

public sealed class TransacaoRepository(IHttpClientFactory httpClientFactory) : ITransacaoRepository
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ApiClient.NomeHttpClient);

    private const string Rota = "api/financas/transacoes";

    public async Task<TransacaoPaginaModel> Listar(TransacaoFiltroModel filtro, CancellationToken cancellationToken)
    {
        var resultado = await httpClient.Enviar<TransacaoPaginaModel>(HttpMethod.Post, $"{Rota}/paginacao", filtro, cancellationToken);
        return resultado.Valor!;
    }

    public async Task<List<TransacaoModel>> ListarTodas(TransacaoFiltroModel filtro, CancellationToken cancellationToken)
    {
        var resultado = await httpClient.Enviar<List<TransacaoModel>>(HttpMethod.Post, $"{Rota}/exportacao", filtro, cancellationToken);
        return resultado.Valor!;
    }

    public Task<TransacaoModel?> BuscarPorCodigo(Guid codigo, CancellationToken cancellationToken)
        => httpClient.BuscarOuNulo<TransacaoModel>($"{Rota}/{codigo}", cancellationToken);

    public Task<Resultado> Criar(TransacaoSalvarModel transacao, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Post, Rota, transacao, cancellationToken);

    public Task<Resultado> Alterar(TransacaoSalvarModel transacao, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Patch, Rota, transacao, cancellationToken);

    public Task<Resultado> Inativar(Guid codigo, CancellationToken cancellationToken)
        => httpClient.Enviar(HttpMethod.Delete, $"{Rota}/{codigo}", corpo: null, cancellationToken);

    public async Task<int> ImportarRecorrentes(ImportarRecorrentesModel importar, CancellationToken cancellationToken)
    {
        var resultado = await httpClient.Enviar<ImportarRecorrentesResultadoModel>(
            HttpMethod.Post, $"{Rota}/importar-recorrentes", importar, cancellationToken);

        return resultado.Valor!.QuantidadeImportada;
    }
}

#region Interfaces

public interface ITransacaoRepository
{
    // Traz junto os totais do mês e quantas recorrentes estão pendentes de importação.
    Task<TransacaoPaginaModel> Listar(TransacaoFiltroModel filtro, CancellationToken cancellationToken);

    // Sem paginação, porque a exportação precisa do filtro inteiro de uma vez.
    Task<List<TransacaoModel>> ListarTodas(TransacaoFiltroModel filtro, CancellationToken cancellationToken);

    Task<TransacaoModel?> BuscarPorCodigo(Guid codigo, CancellationToken cancellationToken);

    // Falha quando a categoria informada não pertence ao usuário.
    Task<Resultado> Criar(TransacaoSalvarModel transacao, CancellationToken cancellationToken);

    Task<Resultado> Alterar(TransacaoSalvarModel transacao, CancellationToken cancellationToken);

    Task<Resultado> Inativar(Guid codigo, CancellationToken cancellationToken);

    // Copia as recorrentes do mês anterior e devolve quantas foram criadas.
    Task<int> ImportarRecorrentes(ImportarRecorrentesModel importar, CancellationToken cancellationToken);
}

#endregion
