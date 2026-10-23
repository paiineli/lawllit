using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;

namespace Lawllit.Api.Finance.Services;

public sealed class TransacaoService(
    ITransacaoRepository transacaoRepositorio,
    ICategoriaRepository categoriaRepositorio) : ITransacaoService
{
    public async Task<TransacaoPaginaModel> Listar(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken)
    {
        var (mes, ano) = NormalizarPeriodo(filtro);

        var pagina = await transacaoRepositorio.Listar(cdUsuario, filtro, cancellationToken);
        var totais = await transacaoRepositorio.TotaisDoFiltro(cdUsuario, filtro, cancellationToken);
        var recorrentesPendentes = await transacaoRepositorio.RecorrentesPendentes(
            cdUsuario, mes, ano, cancellationToken);

        return new TransacaoPaginaModel
        {
            Pagina = new PaginacaoResposta<TransacaoItemModel>
            {
                Itens = [.. pagina.Itens.Select(TransacaoItemModel.De)],
                TotalItens = pagina.TotalItens,
                PaginaAtual = pagina.PaginaAtual,
                ItensPorPagina = pagina.ItensPorPagina,
            },
            Mes = mes,
            Ano = ano,
            TotalReceitas = totais.Receitas,
            TotalDespesas = totais.Despesas,
            TotalInvestimentos = totais.Investimentos,
            RecorrentesPendentes = recorrentesPendentes,
        };
    }

    public async Task<TransacaoItemModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken)
        => await transacaoRepositorio.BuscarPorCodigo(cdUsuario, cdTransacao, cancellationToken) is { } transacao
            ? TransacaoItemModel.De(transacao)
            : null;

    // Sem paginação, porque quem exporta quer o período inteiro num arquivo só.
    public async Task<List<TransacaoItemModel>> ListarTodas(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken)
    {
        NormalizarPeriodo(filtro);
        return [.. (await transacaoRepositorio.ListarTodas(cdUsuario, filtro, cancellationToken)).Select(TransacaoItemModel.De)];
    }

    // Mês ou ano que não existem, vindos da URL, viram o mês atual em vez de estourar ao montar a data.
    private static (int Mes, int Ano) NormalizarPeriodo(TransacaoFiltroModel filtro)
    {
        var agora = HorarioBrasil.Agora;

        if (!Validacao.Periodo(filtro.Mes ?? agora.Month, filtro.Ano ?? agora.Year))
        {
            filtro.Mes = null;
            filtro.Ano = null;
        }

        filtro.Mes ??= agora.Month;
        filtro.Ano ??= agora.Year;

        return (filtro.Mes.Value, filtro.Ano.Value);
    }

    public async Task<Resultado> Criar(Guid cdUsuario, TransacaoSalvarModel transacao, CancellationToken cancellationToken)
    {
        if (transacao.Validar() is { } erro)
            return Resultado.Falha(erro);

        var categoria = await categoriaRepositorio.BuscarPorCodigo(cdUsuario, transacao.CodigoCategoria, cancellationToken);
        if (categoria is null)
            return Resultado.Falha("Categoria não encontrada.");

        // A tela só oferece categoria do tipo escolhido, mas a API não pode depender disso.
        if (categoria.TxTipo != transacao.Tipo)
            return Resultado.Falha("A categoria escolhida não é desse tipo de lançamento.");

        await transacaoRepositorio.Criar(new TransacaoModel
        {
            CdTransacao = Guid.NewGuid(),
            TxDescricao = transacao.Descricao?.Trim() ?? string.Empty,
            VlTransacao = transacao.Valor,
            TxTipo = transacao.Tipo,
            DtTransacao = DateTime.SpecifyKind(transacao.Data.Date, DateTimeKind.Utc),
            CdUsuario = cdUsuario,
            CdCategoria = transacao.CodigoCategoria,
            SnRecorrente = transacao.Recorrente ? Constantes.Sim : Constantes.Nao,
        }, cancellationToken);

        return Resultado.Ok();
    }

    public async Task<Resultado> Alterar(Guid cdUsuario, TransacaoSalvarModel transacao, CancellationToken cancellationToken)
    {
        if (transacao.Validar() is { } erro)
            return Resultado.Falha(erro);

        var existente = await transacaoRepositorio.BuscarPorCodigo(cdUsuario, transacao.Codigo, cancellationToken);
        if (existente is null)
            return Resultado.Falha("Transação não encontrada.");

        var categoria = await categoriaRepositorio.BuscarPorCodigo(cdUsuario, transacao.CodigoCategoria, cancellationToken);
        if (categoria is null)
            return Resultado.Falha("Categoria não encontrada.");

        // A tela só oferece categoria do tipo escolhido, mas a API não pode depender disso.
        if (categoria.TxTipo != transacao.Tipo)
            return Resultado.Falha("A categoria escolhida não é desse tipo de lançamento.");

        existente.TxDescricao = transacao.Descricao?.Trim() ?? string.Empty;
        existente.VlTransacao = transacao.Valor;
        existente.TxTipo = transacao.Tipo;
        existente.DtTransacao = DateTime.SpecifyKind(transacao.Data.Date, DateTimeKind.Utc);
        existente.CdCategoria = transacao.CodigoCategoria;
        existente.SnRecorrente = transacao.Recorrente ? Constantes.Sim : Constantes.Nao;

        await transacaoRepositorio.Alterar(existente, cancellationToken);
        return Resultado.Ok();
    }

    public async Task<Resultado> Inativar(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken)
    {
        var transacao = await transacaoRepositorio.BuscarPorCodigo(cdUsuario, cdTransacao, cancellationToken);
        if (transacao is null)
            return Resultado.Falha("Transação não encontrada.");

        await transacaoRepositorio.AlternarAtivo(cdUsuario, cdTransacao, Constantes.Nao, cancellationToken);
        return Resultado.Ok();
    }

    public async Task<Resultado<ImportarRecorrentesResultadoModel>> ImportarRecorrentes(Guid cdUsuario, ImportarRecorrentesModel importar, CancellationToken cancellationToken)
    {
        if (importar.Validar() is { } erro)
            return Resultado<ImportarRecorrentesResultadoModel>.Falha(erro);

        var importadas = await transacaoRepositorio.ImportarRecorrentes(cdUsuario, importar.Mes, importar.Ano, cancellationToken);
        return Resultado<ImportarRecorrentesResultadoModel>.Ok(new ImportarRecorrentesResultadoModel { QuantidadeImportada = importadas });
    }
}

#region Interfaces

public interface ITransacaoService
{
    Task<TransacaoPaginaModel> Listar(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken);
    Task<TransacaoItemModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken);
    Task<List<TransacaoItemModel>> ListarTodas(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken);
    Task<Resultado> Criar(Guid cdUsuario, TransacaoSalvarModel transacao, CancellationToken cancellationToken);
    Task<Resultado> Alterar(Guid cdUsuario, TransacaoSalvarModel transacao, CancellationToken cancellationToken);
    Task<Resultado> Inativar(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken);
    Task<Resultado<ImportarRecorrentesResultadoModel>> ImportarRecorrentes(Guid cdUsuario, ImportarRecorrentesModel importar, CancellationToken cancellationToken);
}

#endregion
