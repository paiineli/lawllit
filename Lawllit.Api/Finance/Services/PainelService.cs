using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;

namespace Lawllit.Api.Finance.Services;

public sealed class PainelService(ITransacaoRepository transacaoRepositorio) : IPainelService
{
    private const int MesesTendenciaMes = 6;
    private const int MesesTendenciaAno = 12;

    // Um mês é ruído e seis demora a reagir, então três é a janela do modo mês.
    private const int MesesReferenciaMes = 3;
    private const int MesesReferenciaAno = 12;

    public async Task<PainelModel> Montar(
        Guid cdUsuario,
        PeriodoPainelEnum periodo,
        int? mesPedido,
        int? anoPedido,
        CancellationToken cancellationToken)
    {
        var agora = DateTime.Now;
        var anoSelecionado = anoPedido ?? agora.Year;
        var mesSelecionado = periodo == PeriodoPainelEnum.ANO ? 1 : mesPedido ?? agora.Month;

        var de = new DateTime(anoSelecionado, mesSelecionado, 1, 0, 0, 0, DateTimeKind.Utc);
        var ate = periodo == PeriodoPainelEnum.ANO ? de.AddYears(1) : de.AddMonths(1);

        // Encosta no período atual, o que deixa uma consulta só resolver os dois.
        var deAnterior = periodo == PeriodoPainelEnum.ANO ? de.AddYears(-1) : de.AddMonths(-1);

        var mesesReferencia = periodo == PeriodoPainelEnum.ANO ? MesesReferenciaAno : MesesReferenciaMes;
        var deReferencia = de.AddMonths(-mesesReferencia);

        var mesesTendencia = periodo == PeriodoPainelEnum.ANO ? MesesTendenciaAno : MesesTendenciaMes;
        var ateMesTendencia = periodo == PeriodoPainelEnum.ANO ? 12 : mesSelecionado;

        var totais = await transacaoRepositorio.TotaisDoPeriodo(cdUsuario, de, ate, deAnterior, cancellationToken);
        var gastoPorCategoria = await transacaoRepositorio.GastoPorCategoria(cdUsuario, de, ate, deReferencia, mesesReferencia, cancellationToken);
        var tendencia = await transacaoRepositorio.TendenciaMensal(cdUsuario, ateMesTendencia, anoSelecionado, mesesTendencia, cancellationToken);
        var investido = await transacaoRepositorio.TotalInvestido(cdUsuario, ate, cancellationToken);

        var mesCorrente = periodo == PeriodoPainelEnum.MES && mesSelecionado == agora.Month && anoSelecionado == agora.Year;

        // Só faz sentido no mês: no ano viraria "o resto do ano", que não ajuda a decidir nada.
        var despesasAVencer = periodo == PeriodoPainelEnum.MES
            ? await transacaoRepositorio.DespesasAVencer(cdUsuario, de, ate, cancellationToken)
            : 0m;

        var painel = new PainelModel
        {
            Periodo = periodo,
            Mes = periodo == PeriodoPainelEnum.ANO ? 0 : mesSelecionado,
            Ano = anoSelecionado,
            TotalReceitas = totais.TotalReceitas,
            TotalDespesas = totais.TotalDespesas,
            TotalInvestimentos = totais.TotalInvestimentos,
            Saldo = totais.Saldo,
            DespesasRecorrentes = totais.DespesasRecorrentes,
            InvestimentosRecorrentes = totais.InvestimentosRecorrentes,
            ReceitasAnteriores = totais.ReceitasAnteriores,
            DespesasAnteriores = totais.DespesasAnteriores,
            InvestimentosAnteriores = totais.InvestimentosAnteriores,
            DespesasPorCategoria = MontarComparativos(gastoPorCategoria),
            MesesDeReferencia = mesesReferencia,
            TendenciaMensal = tendencia,
            DespesasAVencer = despesasAVencer,
            TotalInvestido = investido.Total,
            MesesInvestidos = investido.QuantidadeMeses,
            MesCorrente = mesCorrente,
        };

        AplicarTaxaPoupanca(painel);
        AplicarRitmo(painel, periodo, mesSelecionado, anoSelecionado, agora, mesCorrente);
        AplicarNavegacao(painel, periodo, de, agora);

        return painel;
    }

    private static List<ComparativoCategoriaModel> MontarComparativos(List<GastoCategoriaModel> gastos)
        => gastos
            .Select(categoria => new ComparativoCategoriaModel(
                categoria.NomeCategoria,
                categoria.Total,
                categoria.MediaReferencia,
                categoria.MediaReferencia > 0
                    ? (categoria.Total - categoria.MediaReferencia) / categoria.MediaReferencia * 100
                    : null))
            .ToList();

    // O saldo desconta o aporte porque ele sai do caixa; a poupança não, porque foi guardado.
    private static void AplicarTaxaPoupanca(PainelModel painel)
    {
        if (painel.TotalReceitas <= 0) return;

        painel.TaxaPoupanca = (painel.TotalReceitas - painel.TotalDespesas) / painel.TotalReceitas * 100;

        // Só o recorrente compromete a renda; aporte esporádico é escolha daquele mês.
        painel.ParcelaComprometida = painel.CompromissoRecorrente / painel.TotalReceitas * 100;
    }

    private static void AplicarRitmo(
        PainelModel painel,
        PeriodoPainelEnum periodo,
        int mesSelecionado,
        int anoSelecionado,
        DateTime agora,
        bool mesCorrente)
    {
        if (periodo == PeriodoPainelEnum.ANO) return;

        var diasNoMes = DateTime.DaysInMonth(anoSelecionado, mesSelecionado);
        var diasDecorridos = mesCorrente ? agora.Day : diasNoMes;

        // Só o que já venceu entra na média, senão despesa agendada infla a projeção.
        var despesasVencidas = mesCorrente
            ? painel.TotalDespesas - painel.DespesasAVencer
            : painel.TotalDespesas;

        painel.DiasNoMes = diasNoMes;
        painel.DiasDecorridos = diasDecorridos;
        painel.MediaDiaria = despesasVencidas > 0 ? despesasVencidas / diasDecorridos : 0;

        painel.ProjecaoMensal = mesCorrente && despesasVencidas > 0
            ? despesasVencidas / diasDecorridos * diasNoMes
            : null;
    }

    // Avança até um ano à frente, porque a tela de transações deixa lançar no futuro.
    private static void AplicarNavegacao(PainelModel painel, PeriodoPainelEnum periodo, DateTime de, DateTime agora)
    {
        var anterior = periodo == PeriodoPainelEnum.ANO ? de.AddYears(-1) : de.AddMonths(-1);
        var proximo = periodo == PeriodoPainelEnum.ANO ? de.AddYears(1) : de.AddMonths(1);

        painel.MesAnterior = anterior.Month;
        painel.AnoAnterior = anterior.Year;
        painel.ProximoMes = proximo.Month;
        painel.ProximoAno = proximo.Year;

        var limiteFrente = periodo == PeriodoPainelEnum.ANO
            ? new DateTime(agora.Year + 1, 1, 1)
            : new DateTime(agora.Year, agora.Month, 1).AddMonths(12);

        painel.PodeAvancar = proximo.Date <= limiteFrente.Date;
    }
}

#region Interfaces

public interface IPainelService
{
    Task<PainelModel> Montar(
        Guid cdUsuario,
        PeriodoPainelEnum periodo,
        int? mesPedido,
        int? anoPedido,
        CancellationToken cancellationToken);
}

#endregion
