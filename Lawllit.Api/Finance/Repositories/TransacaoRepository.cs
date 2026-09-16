using Npgsql;
using Dapper;
using Lawllit.Api.Common;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;

namespace Lawllit.Api.Finance.Repositories;

public sealed class TransacaoRepository(NpgsqlDataSource fonteDados) : ITransacaoRepository
{
    // O Dapper mapeia por nome de coluna, então um record nomeado em vez de tupla.
    private sealed record TotaisFiltro(decimal Receitas, decimal Despesas, decimal Investimentos);

    // Corta em NM_CATEGORIA porque CD_CATEGORIA existe nas duas tabelas e cortaria cedo demais.
    private const string ColunaCorte = "NM_CATEGORIA";

    private const string SelectComCategoria = """
        SELECT
            t.CD_TRANSACAO, t.TX_DESCRICAO, t.VL_TRANSACAO, t.TX_TIPO, t.DT_TRANSACAO,
            t.SN_RECORRENTE, t.SN_ATIVO, t.DT_CADASTRO, t.CD_USUARIO, t.CD_CATEGORIA,
            c.NM_CATEGORIA, c.CD_CATEGORIA, c.TX_TIPO, c.CD_USUARIO, c.SN_ATIVO, c.DT_CADASTRO
        FROM TRANSACAO t, CATEGORIA c
        WHERE t.CD_CATEGORIA = c.CD_CATEGORIA
          AND t.SN_ATIVO = 'S'
        """;

    private static TransacaoModel Juntar(TransacaoModel transacao, CategoriaModel categoria)
    {
        transacao.Categoria = categoria;
        return transacao;
    }

    // Um lugar só, porque a página, a contagem, os totais e a exportação usam o mesmo filtro.
    private static (string Where, DynamicParameters Parametros) MontarFiltro(Guid cdUsuario, TransacaoFiltroModel filtro)
    {
        var where = """
              AND t.CD_USUARIO = @CdUsuario
            """;

        var parametros = new DynamicParameters();
        parametros.Add("CdUsuario", cdUsuario);

        if (filtro.Tipo.HasValue)
        {
            where += """

                  AND t.TX_TIPO = @TxTipo
                """;
            parametros.Add("TxTipo", filtro.Tipo.Value.ToString());
        }

        if (filtro.Mes.HasValue)
        {
            where += """

                  AND EXTRACT(MONTH FROM t.DT_TRANSACAO) = @Mes
                """;
            parametros.Add("Mes", filtro.Mes.Value);
        }

        if (filtro.Ano.HasValue)
        {
            where += """

                  AND EXTRACT(YEAR FROM t.DT_TRANSACAO) = @Ano
                """;
            parametros.Add("Ano", filtro.Ano.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            where += """

                  AND t.TX_DESCRICAO ILIKE @Busca
                """;
            parametros.Add("Busca", $"%{filtro.Busca.Trim()}%");
        }

        return (where, parametros);
    }

    public async Task<PaginacaoResposta<TransacaoModel>> Listar(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken)
    {
        var (where, parametros) = MontarFiltro(cdUsuario, filtro);

        var consultaPagina = $"""
            {SelectComCategoria}
            {where}
            ORDER BY t.DT_TRANSACAO DESC, t.DT_CADASTRO DESC
            OFFSET @Offset ROWS FETCH NEXT @ItensPorPagina ROWS ONLY
            """;

        var consultaTotal = $"""
            SELECT COUNT(1)
            FROM TRANSACAO t
            WHERE t.SN_ATIVO = 'S'
            {where}
            """;

        parametros.Add("Offset", filtro.Paginacao.Offset);
        parametros.Add("ItensPorPagina", filtro.Paginacao.ItensPorPagina);

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);

        var itens = await conexao.QueryAsync<TransacaoModel, CategoriaModel, TransacaoModel>(
            new CommandDefinition(consultaPagina, parametros, cancellationToken: cancellationToken),
            Juntar,
            splitOn: ColunaCorte);

        var totalItens = await conexao.ExecuteScalarAsync<int>(
            new CommandDefinition(consultaTotal, parametros, cancellationToken: cancellationToken));

        return new PaginacaoResposta<TransacaoModel>
        {
            Itens = itens.AsList(),
            TotalItens = totalItens,
            PaginaAtual = filtro.Paginacao.PaginaAtual,
            ItensPorPagina = filtro.Paginacao.ItensPorPagina,
        };
    }

    public async Task<List<TransacaoModel>> ListarTodas(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken)
    {
        var (where, parametros) = MontarFiltro(cdUsuario, filtro);

        var consulta = $"""
            {SelectComCategoria}
            {where}
            ORDER BY t.DT_TRANSACAO ASC, t.DT_CADASTRO ASC
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);

        var transacoes = await conexao.QueryAsync<TransacaoModel, CategoriaModel, TransacaoModel>(
            new CommandDefinition(consulta, parametros, cancellationToken: cancellationToken),
            Juntar,
            splitOn: ColunaCorte);

        return transacoes.AsList();
    }

    public async Task<(decimal Receitas, decimal Despesas, decimal Investimentos)> TotaisDoFiltro(
        Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken)
    {
        var (where, parametros) = MontarFiltro(cdUsuario, filtro);

        var consulta = $"""
            SELECT
                COALESCE(SUM(CASE WHEN t.TX_TIPO = 'RECEITA'      THEN t.VL_TRANSACAO ELSE 0 END), 0) AS Receitas,
                COALESCE(SUM(CASE WHEN t.TX_TIPO = 'DESPESA'      THEN t.VL_TRANSACAO ELSE 0 END), 0) AS Despesas,
                COALESCE(SUM(CASE WHEN t.TX_TIPO = 'INVESTIMENTO' THEN t.VL_TRANSACAO ELSE 0 END), 0) AS Investimentos
            FROM TRANSACAO t
            WHERE t.SN_ATIVO = 'S'
            {where}
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);

        var totais = await conexao.QueryFirstAsync<TotaisFiltro>(
            new CommandDefinition(consulta, parametros, cancellationToken: cancellationToken));

        return (totais.Receitas, totais.Despesas, totais.Investimentos);
    }

    public async Task<TransacaoModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken)
    {
        var consulta = $"""
            {SelectComCategoria}
              AND t.CD_USUARIO   = @CdUsuario
              AND t.CD_TRANSACAO = @CdTransacao
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        var transacoes = await conexao.QueryAsync<TransacaoModel, CategoriaModel, TransacaoModel>(
            new CommandDefinition(
                consulta,
                new { CdUsuario = cdUsuario, CdTransacao = cdTransacao },
                cancellationToken: cancellationToken),
            Juntar,
            splitOn: ColunaCorte);

        return transacoes.FirstOrDefault();
    }

    // Range único em vez de EXTRACT(MONTH), que descartaria o índice e varreria a tabela.
    public async Task<TotaisPeriodoModel> TotaisDoPeriodo(
        Guid cdUsuario,
        DateTime de,
        DateTime ate,
        DateTime deAnterior,
        CancellationToken cancellationToken)
    {
        const string consulta = """
            SELECT
                COALESCE(SUM(CASE WHEN DT_TRANSACAO >= @De         AND DT_TRANSACAO < @Ate AND TX_TIPO = 'RECEITA'      THEN VL_TRANSACAO ELSE 0 END), 0) AS TotalReceitas,
                COALESCE(SUM(CASE WHEN DT_TRANSACAO >= @De         AND DT_TRANSACAO < @Ate AND TX_TIPO = 'DESPESA'      THEN VL_TRANSACAO ELSE 0 END), 0) AS TotalDespesas,
                COALESCE(SUM(CASE WHEN DT_TRANSACAO >= @De         AND DT_TRANSACAO < @Ate AND TX_TIPO = 'INVESTIMENTO' THEN VL_TRANSACAO ELSE 0 END), 0) AS TotalInvestimentos,
                COALESCE(SUM(CASE WHEN DT_TRANSACAO >= @De         AND DT_TRANSACAO < @Ate AND TX_TIPO = 'DESPESA'      AND SN_RECORRENTE = 'S' THEN VL_TRANSACAO ELSE 0 END), 0) AS DespesasRecorrentes,
                COALESCE(SUM(CASE WHEN DT_TRANSACAO >= @De         AND DT_TRANSACAO < @Ate AND TX_TIPO = 'INVESTIMENTO' AND SN_RECORRENTE = 'S' THEN VL_TRANSACAO ELSE 0 END), 0) AS InvestimentosRecorrentes,
                COALESCE(SUM(CASE WHEN DT_TRANSACAO >= @DeAnterior AND DT_TRANSACAO < @De  AND TX_TIPO = 'RECEITA'      THEN VL_TRANSACAO ELSE 0 END), 0) AS ReceitasAnteriores,
                COALESCE(SUM(CASE WHEN DT_TRANSACAO >= @DeAnterior AND DT_TRANSACAO < @De  AND TX_TIPO = 'DESPESA'      THEN VL_TRANSACAO ELSE 0 END), 0) AS DespesasAnteriores,
                COALESCE(SUM(CASE WHEN DT_TRANSACAO >= @DeAnterior AND DT_TRANSACAO < @De  AND TX_TIPO = 'INVESTIMENTO' THEN VL_TRANSACAO ELSE 0 END), 0) AS InvestimentosAnteriores
            FROM TRANSACAO
            WHERE CD_USUARIO    = @CdUsuario
              AND SN_ATIVO      = 'S'
              AND DT_TRANSACAO >= @DeAnterior
              AND DT_TRANSACAO  < @Ate
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);

        return await conexao.QueryFirstAsync<TotaisPeriodoModel>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, De = de, Ate = ate, DeAnterior = deAnterior },
            cancellationToken: cancellationToken));
    }

    // O HAVING descarta categoria sem gasto no período, que viraria fatia zerada no gráfico.
    public async Task<List<GastoCategoriaModel>> GastoPorCategoria(
        Guid cdUsuario,
        DateTime de,
        DateTime ate,
        DateTime deReferencia,
        int mesesReferencia,
        CancellationToken cancellationToken)
    {
        const string consulta = """
            SELECT
                c.NM_CATEGORIA AS NomeCategoria,
                COALESCE(SUM(CASE WHEN t.DT_TRANSACAO >= @De THEN t.VL_TRANSACAO ELSE 0 END), 0) AS Total,
                COALESCE(
                    SUM(CASE WHEN t.DT_TRANSACAO < @De THEN t.VL_TRANSACAO ELSE 0 END)
                        / NULLIF(@MesesReferencia, 0),
                    0) AS MediaReferencia
            FROM TRANSACAO t, CATEGORIA c
            WHERE t.CD_CATEGORIA = c.CD_CATEGORIA
              AND t.CD_USUARIO   = @CdUsuario
              AND t.SN_ATIVO     = 'S'
              AND t.TX_TIPO      = 'DESPESA'
              AND t.DT_TRANSACAO >= @DeReferencia
              AND t.DT_TRANSACAO  < @Ate
            GROUP BY c.NM_CATEGORIA
            HAVING COALESCE(SUM(CASE WHEN t.DT_TRANSACAO >= @De THEN t.VL_TRANSACAO ELSE 0 END), 0) > 0
            ORDER BY Total DESC
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);

        var linhas = await conexao.QueryAsync<GastoCategoriaModel>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, De = de, Ate = ate, DeReferencia = deReferencia, MesesReferencia = mesesReferencia },
            cancellationToken: cancellationToken));

        return linhas.AsList();
    }

    // Única consulta sem recorte de período: é o acumulado até o fim do período em tela.
    public async Task<TotalInvestidoModel> TotalInvestido(Guid cdUsuario, DateTime ate, CancellationToken cancellationToken)
    {
        const string consulta = """
            SELECT
                COALESCE(SUM(VL_TRANSACAO), 0)                              AS Total,
                COUNT(DISTINCT DATE_TRUNC('month', DT_TRANSACAO))::int      AS QuantidadeMeses
            FROM TRANSACAO
            WHERE CD_USUARIO   = @CdUsuario
              AND SN_ATIVO     = 'S'
              AND TX_TIPO      = 'INVESTIMENTO'
              AND DT_TRANSACAO < @Ate
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);

        return await conexao.QueryFirstAsync<TotalInvestidoModel>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, Ate = ate },
            cancellationToken: cancellationToken));
    }

    public async Task<decimal> DespesasAVencer(Guid cdUsuario, DateTime de, DateTime ate, CancellationToken cancellationToken)
    {
        var hoje = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

        const string consulta = """
            SELECT COALESCE(SUM(VL_TRANSACAO), 0)
            FROM TRANSACAO
            WHERE CD_USUARIO    = @CdUsuario
              AND SN_ATIVO      = 'S'
              AND TX_TIPO       = 'DESPESA'
              AND DT_TRANSACAO >= @De
              AND DT_TRANSACAO  < @Ate
              AND DT_TRANSACAO  > @Hoje
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        return await conexao.ExecuteScalarAsync<decimal>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, De = de, Ate = ate, Hoje = hoje },
            cancellationToken: cancellationToken));
    }

    public async Task<int> RecorrentesPendentes(Guid cdUsuario, int mes, int ano, CancellationToken cancellationToken)
    {
        const string consulta = """
            SELECT COUNT(1)
            FROM TRANSACAO
            WHERE CD_USUARIO                       = @CdUsuario
              AND SN_ATIVO                         = 'S'
              AND SN_RECORRENTE                    = 'S'
              AND EXTRACT(MONTH FROM DT_TRANSACAO) = @Mes
              AND EXTRACT(YEAR  FROM DT_TRANSACAO) = @Ano
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);

        var recorrentesDoMes = await conexao.ExecuteScalarAsync<int>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, Mes = mes, Ano = ano },
            cancellationToken: cancellationToken));

        // Já existe recorrente lançado no mês, então não há nada pendente para importar.
        if (recorrentesDoMes > 0) return 0;

        var mesAnterior = new DateTime(ano, mes, 1).AddMonths(-1);

        return await conexao.ExecuteScalarAsync<int>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, Mes = mesAnterior.Month, Ano = mesAnterior.Year },
            cancellationToken: cancellationToken));
    }

    public async Task<List<TransacaoModel>> RecorrentesParaImportar(Guid cdUsuario, int mes, int ano, CancellationToken cancellationToken)
    {
        var mesAnterior = new DateTime(ano, mes, 1).AddMonths(-1);

        var consulta = $"""
            {SelectComCategoria}
              AND t.CD_USUARIO                       = @CdUsuario
              AND t.SN_RECORRENTE                    = 'S'
              AND EXTRACT(MONTH FROM t.DT_TRANSACAO) = @Mes
              AND EXTRACT(YEAR  FROM t.DT_TRANSACAO) = @Ano
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        var transacoes = await conexao.QueryAsync<TransacaoModel, CategoriaModel, TransacaoModel>(
            new CommandDefinition(
                consulta,
                new { CdUsuario = cdUsuario, Mes = mesAnterior.Month, Ano = mesAnterior.Year },
                cancellationToken: cancellationToken),
            Juntar,
            splitOn: ColunaCorte);

        return transacoes.AsList();
    }

    public async Task<List<TendenciaMensalModel>> TendenciaMensal(Guid cdUsuario, int ateMes, int ateAno, int quantidadeMeses, CancellationToken cancellationToken)
    {
        var meses = new List<(int Mes, int Ano)>();
        var mesAtual = ateMes;
        var anoAtual = ateAno;

        for (var indice = 0; indice < quantidadeMeses; indice++)
        {
            meses.Add((mesAtual, anoAtual));
            if (--mesAtual == 0) { mesAtual = 12; anoAtual--; }
        }

        meses.Reverse();

        var dataInicio = new DateTime(meses[0].Ano, meses[0].Mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var dataFim = new DateTime(ateAno, ateMes, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);

        const string consulta = """
            SELECT
                EXTRACT(MONTH FROM DT_TRANSACAO)::int AS Mes,
                EXTRACT(YEAR  FROM DT_TRANSACAO)::int AS Ano,
                COALESCE(SUM(CASE WHEN TX_TIPO = 'RECEITA'      THEN VL_TRANSACAO ELSE 0 END), 0) AS Receitas,
                COALESCE(SUM(CASE WHEN TX_TIPO = 'DESPESA'      THEN VL_TRANSACAO ELSE 0 END), 0) AS Despesas,
                COALESCE(SUM(CASE WHEN TX_TIPO = 'INVESTIMENTO' THEN VL_TRANSACAO ELSE 0 END), 0) AS Investimentos
            FROM TRANSACAO
            WHERE CD_USUARIO    = @CdUsuario
              AND SN_ATIVO      = 'S'
              AND DT_TRANSACAO >= @DataInicio
              AND DT_TRANSACAO  < @DataFim
            GROUP BY EXTRACT(YEAR FROM DT_TRANSACAO), EXTRACT(MONTH FROM DT_TRANSACAO)
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        var linhas = await conexao.QueryAsync<TendenciaMensalModel>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, DataInicio = dataInicio, DataFim = dataFim },
            cancellationToken: cancellationToken));

        var porMes = linhas.ToDictionary(linha => (linha.Mes, linha.Ano));

        // Mês sem lançamento não volta do banco, mas o gráfico precisa da série completa.
        return meses
            .Select(mes => porMes.TryGetValue((mes.Mes, mes.Ano), out var tendencia)
                ? tendencia
                : new TendenciaMensalModel(mes.Mes, mes.Ano, 0, 0, 0))
            .ToList();
    }

    public async Task Criar(TransacaoModel transacao, CancellationToken cancellationToken)
    {
        const string consulta = """
            INSERT INTO TRANSACAO (
                CD_TRANSACAO, TX_DESCRICAO, VL_TRANSACAO, TX_TIPO, DT_TRANSACAO,
                SN_RECORRENTE, CD_USUARIO, CD_CATEGORIA
            ) VALUES (
                @CdTransacao, @TxDescricao, @VlTransacao, @TxTipo, @DtTransacao,
                @SnRecorrente, @CdUsuario, @CdCategoria
            )
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(
            consulta,
            new
            {
                transacao.CdTransacao,
                transacao.TxDescricao,
                transacao.VlTransacao,
                TxTipo = transacao.TxTipo.ToString(),
                transacao.DtTransacao,
                transacao.SnRecorrente,
                transacao.CdUsuario,
                transacao.CdCategoria,
            },
            cancellationToken: cancellationToken));
    }

    public async Task Alterar(TransacaoModel transacao, CancellationToken cancellationToken)
    {
        const string consulta = """
            UPDATE TRANSACAO SET
                TX_DESCRICAO  = @TxDescricao,
                VL_TRANSACAO  = @VlTransacao,
                TX_TIPO       = @TxTipo,
                DT_TRANSACAO  = @DtTransacao,
                SN_RECORRENTE = @SnRecorrente,
                CD_CATEGORIA  = @CdCategoria
            WHERE CD_TRANSACAO = @CdTransacao
              AND CD_USUARIO   = @CdUsuario
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(
            consulta,
            new
            {
                transacao.TxDescricao,
                transacao.VlTransacao,
                TxTipo = transacao.TxTipo.ToString(),
                transacao.DtTransacao,
                transacao.SnRecorrente,
                transacao.CdCategoria,
                transacao.CdTransacao,
                transacao.CdUsuario,
            },
            cancellationToken: cancellationToken));
    }

    public async Task AlternarAtivo(Guid cdUsuario, Guid cdTransacao, string snAtivo, CancellationToken cancellationToken)
    {
        const string consulta = """
            UPDATE TRANSACAO SET
                SN_ATIVO = @SnAtivo
            WHERE CD_TRANSACAO = @CdTransacao
              AND CD_USUARIO   = @CdUsuario
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, CdTransacao = cdTransacao, SnAtivo = snAtivo },
            cancellationToken: cancellationToken));
    }
}

#region Interfaces

public interface ITransacaoRepository
{
    Task<PaginacaoResposta<TransacaoModel>> Listar(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken);
    Task<List<TransacaoModel>> ListarTodas(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken);
    Task<(decimal Receitas, decimal Despesas, decimal Investimentos)> TotaisDoFiltro(Guid cdUsuario, TransacaoFiltroModel filtro, CancellationToken cancellationToken);
    Task<TransacaoModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdTransacao, CancellationToken cancellationToken);
    Task<TotaisPeriodoModel> TotaisDoPeriodo(Guid cdUsuario, DateTime de, DateTime ate, DateTime deAnterior, CancellationToken cancellationToken);
    Task<List<GastoCategoriaModel>> GastoPorCategoria(Guid cdUsuario, DateTime de, DateTime ate, DateTime deReferencia, int mesesReferencia, CancellationToken cancellationToken);
    Task<TotalInvestidoModel> TotalInvestido(Guid cdUsuario, DateTime ate, CancellationToken cancellationToken);
    Task<decimal> DespesasAVencer(Guid cdUsuario, DateTime de, DateTime ate, CancellationToken cancellationToken);
    Task<int> RecorrentesPendentes(Guid cdUsuario, int mes, int ano, CancellationToken cancellationToken);
    Task<List<TransacaoModel>> RecorrentesParaImportar(Guid cdUsuario, int mes, int ano, CancellationToken cancellationToken);
    Task<List<TendenciaMensalModel>> TendenciaMensal(Guid cdUsuario, int ateMes, int ateAno, int quantidadeMeses, CancellationToken cancellationToken);
    Task Criar(TransacaoModel transacao, CancellationToken cancellationToken);
    Task Alterar(TransacaoModel transacao, CancellationToken cancellationToken);
    Task AlternarAtivo(Guid cdUsuario, Guid cdTransacao, string snAtivo, CancellationToken cancellationToken);
}

#endregion
