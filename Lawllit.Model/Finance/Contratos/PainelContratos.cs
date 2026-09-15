using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance.Contratos;

public sealed class PainelModel
{
    public PeriodoPainelEnum Periodo { get; set; }

    // No modo ano o mês vem zero, e quem decide o que mostrar é o Periodo, não o valor do mês.
    public int Mes { get; set; }
    public int Ano { get; set; }

    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalInvestimentos { get; set; }
    public decimal Saldo { get; set; }
    public decimal DespesasAVencer { get; set; }

    public List<ComparativoCategoriaModel> DespesasPorCategoria { get; set; } = [];
    public List<TendenciaMensalModel> TendenciaMensal { get; set; } = [];

    // Quantos meses entraram na média histórica das categorias.
    public int MesesDeReferencia { get; set; }

    public bool MesCorrente { get; set; }
    public int DiasNoMes { get; set; }
    public int DiasDecorridos { get; set; }
    public decimal MediaDiaria { get; set; }
    public decimal? ProjecaoMensal { get; set; }

    // Quanto do que entrou não foi consumido. Aporte conta como poupado, então fica de fora.
    public decimal? TaxaPoupanca { get; set; }

    // O recorrente é o compromisso do período, o resto é escolha. Vale para despesa e para aporte.
    public decimal DespesasRecorrentes { get; set; }
    public decimal InvestimentosRecorrentes { get; set; }
    public decimal? ParcelaComprometida { get; set; }

    // Total aportado até o fim do período, sem recorte de mês, e em quantos meses.
    public decimal TotalInvestido { get; set; }
    public int MesesInvestidos { get; set; }

    public decimal ReceitasAnteriores { get; set; }
    public decimal DespesasAnteriores { get; set; }
    public decimal InvestimentosAnteriores { get; set; }

    public int MesAnterior { get; set; }
    public int AnoAnterior { get; set; }
    public int ProximoMes { get; set; }
    public int ProximoAno { get; set; }
    public bool PodeAvancar { get; set; }

    public decimal DespesasVariaveis => TotalDespesas - DespesasRecorrentes;
    public decimal InvestimentosVariaveis => TotalInvestimentos - InvestimentosRecorrentes;

    // Custo e aporte em linhas separadas, senão a barra somaria gasto com dinheiro guardado.
    public decimal TotalSaidas => TotalDespesas + TotalInvestimentos;

    // Só o pedaço recorrente compromete a renda. Aporte extra é decisão daquele mês.
    public decimal CompromissoRecorrente => DespesasRecorrentes + InvestimentosRecorrentes;

    public decimal ParcelaSaida(decimal valor) => TotalSaidas > 0 ? valor / TotalSaidas * 100 : 0;

    public decimal TotalRanking => DespesasPorCategoria.Sum(categoria => categoria.Total);

    public decimal? MediaInvestidaPorMes => MesesInvestidos > 0
        ? TotalInvestido / MesesInvestidos
        : null;

    public decimal? VariacaoReceitas => Variacao(TotalReceitas, ReceitasAnteriores);
    public decimal? VariacaoDespesas => Variacao(TotalDespesas, DespesasAnteriores);
    public decimal? VariacaoInvestimentos => Variacao(TotalInvestimentos, InvestimentosAnteriores);

    // Zero para zero não é queda de 100%, é ausência de dado, por isso nulo e não um número.
    private static decimal? Variacao(decimal atual, decimal anterior)
        => anterior > 0 ? (atual - anterior) / anterior * 100 : null;
}

// Gasto da categoria contra a média dela nos meses anteriores, tirada do próprio histórico.
public sealed record GastoCategoriaModel(string NomeCategoria, decimal Total, decimal MediaReferencia);

// O mesmo par acima já com a variação calculada, nula quando não há histórico com que comparar.
public sealed record ComparativoCategoriaModel(
    string NomeCategoria,
    decimal Total,
    decimal MediaReferencia,
    decimal? VariacaoPercentual);

public sealed record TendenciaMensalModel(int Mes, int Ano, decimal Receitas, decimal Despesas, decimal Investimentos);

// Período em tela e período anterior no mesmo registro, porque uma consulta só resolve os dois.
public sealed record TotaisPeriodoModel(
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal TotalInvestimentos,
    decimal DespesasRecorrentes,
    decimal InvestimentosRecorrentes,
    decimal ReceitasAnteriores,
    decimal DespesasAnteriores,
    decimal InvestimentosAnteriores
)
{
    // Investimento sai do caixa do período igual a uma despesa, então abate do saldo.
    public decimal Saldo => TotalReceitas - TotalDespesas - TotalInvestimentos;
}

public sealed record TotalInvestidoModel(decimal Total, int QuantidadeMeses);
