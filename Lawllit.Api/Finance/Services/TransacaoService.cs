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
        var agora = DateTime.Now;
        filtro.Mes ??= agora.Month;
        filtro.Ano ??= agora.Year;

        var pagina = await transacaoRepositorio.Listar(cdUsuario, filtro, cancellationToken);
        var totais = await transacaoRepositorio.TotaisDoFiltro(cdUsuario, filtro, cancellationToken);
        var recorrentesPendentes = await transacaoRepositorio.RecorrentesPendentes(
            cdUsuario, filtro.Mes.Value, filtro.Ano.Value, cancellationToken);

        return new TransacaoPaginaModel
        {
            Pagina = pagina,
            Mes = filtro.Mes.Value,
            Ano = filtro.Ano.Value,
            TotalReceitas = totais.Receitas,
            TotalDespesas = totais.Despesas,
            TotalInvestimentos = totais.Investimentos,
            RecorrentesPendentes = recorrentesPendentes,
        };
    }

    public Task<TransacaoModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken)
        => transacaoRepositorio.BuscarPorCodigo(cdUsuario, cdTransacao, cancellationToken);

    // Sem paginação, porque quem exporta quer o período inteiro num arquivo só.
    public Task<List<TransacaoModel>> ListarTodas(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken)
        => transacaoRepositorio.ListarTodas(cdUsuario, filtro, cancellationToken);

    public async Task<Resultado> Criar(Guid cdUsuario, TransacaoSalvarModel transacao, CancellationToken cancellationToken)
    {
        var categoria = await categoriaRepositorio.BuscarPorCodigo(cdUsuario, transacao.CodigoCategoria, cancellationToken);
        if (categoria is null)
            return Resultado.Falha("Categoria não encontrada.");

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
        var existente = await transacaoRepositorio.BuscarPorCodigo(cdUsuario, transacao.Codigo, cancellationToken);
        if (existente is null)
            return Resultado.Falha("Transação não encontrada.");

        var categoria = await categoriaRepositorio.BuscarPorCodigo(cdUsuario, transacao.CodigoCategoria, cancellationToken);
        if (categoria is null)
            return Resultado.Falha("Categoria não encontrada.");

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

    public async Task<ImportarRecorrentesResultadoModel> ImportarRecorrentes(Guid cdUsuario, ImportarRecorrentesModel importar, CancellationToken cancellationToken)
    {
        var anteriores = await transacaoRepositorio.RecorrentesParaImportar(cdUsuario, importar.Mes, importar.Ano, cancellationToken);

        foreach (var anterior in anteriores)
        {
            // Dia 31 virando fevereiro cai no último dia válido do mês de destino.
            var dia = Math.Min(anterior.DtTransacao.Day, DateTime.DaysInMonth(importar.Ano, importar.Mes));

            await transacaoRepositorio.Criar(new TransacaoModel
            {
                CdTransacao = Guid.NewGuid(),
                TxDescricao = anterior.TxDescricao,
                VlTransacao = anterior.VlTransacao,
                TxTipo = anterior.TxTipo,
                DtTransacao = DateTime.SpecifyKind(new DateTime(importar.Ano, importar.Mes, dia), DateTimeKind.Utc),
                CdUsuario = cdUsuario,
                CdCategoria = anterior.CdCategoria,
                SnRecorrente = Constantes.Sim,
            }, cancellationToken);
        }

        return new ImportarRecorrentesResultadoModel { QuantidadeImportada = anteriores.Count };
    }
}

#region Interfaces

public interface ITransacaoService
{
    Task<TransacaoPaginaModel> Listar(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken);
    Task<TransacaoModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken);
    Task<List<TransacaoModel>> ListarTodas(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken);
    Task<Resultado> Criar(Guid cdUsuario, TransacaoSalvarModel transacao, CancellationToken cancellationToken);
    Task<Resultado> Alterar(Guid cdUsuario, TransacaoSalvarModel transacao, CancellationToken cancellationToken);
    Task<Resultado> Inativar(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken);
    Task<ImportarRecorrentesResultadoModel> ImportarRecorrentes(Guid cdUsuario, ImportarRecorrentesModel importar, CancellationToken cancellationToken);
}

#endregion
