using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance.Contracts;

public sealed class DashboardMOD
{
    public DashboardPeriodEnum Period { get; set; }

    // No modo ano o mês perde sentido e vem zero. A view usa o Period para decidir
    // o que mostrar, em vez de adivinhar pelo valor do mês.
    public int Month { get; set; }
    public int Year { get; set; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalInvestments { get; set; }
    public decimal Balance { get; set; }
    public decimal UpcomingExpenses { get; set; }

    public List<CategoryComparisonMOD> ExpensesByCategory { get; set; } = [];
    public List<MonthlyTrendMOD> MonthlyTrend { get; set; } = [];

    /// <summary>Quantos meses entraram na média histórica das categorias.</summary>
    public int ReferenceMonthCount { get; set; }

    public bool IsCurrentMonth { get; set; }
    public int DaysInMonth { get; set; }
    public int DaysDone { get; set; }
    public decimal DailyAverage { get; set; }
    public decimal? MonthlyProjection { get; set; }

    // Quanto do que entrou não foi consumido. Aporte conta como poupado e não como
    // gasto, então a conta ignora o investimento, diferente do saldo em caixa.
    public decimal? SavingsRate { get; set; }

    // O que está marcado como recorrente é o compromisso do período, o resto é escolha.
    // Vale para despesa e para aporte, os dois voltam sozinhos todo mês pela importação.
    public decimal RecurringExpenses { get; set; }
    public decimal RecurringInvestments { get; set; }
    public decimal? CommittedShare { get; set; }

    // Total aportado até o fim do período, sem recorte de mês, e em quantos meses.
    public decimal TotalInvested { get; set; }
    public int InvestedMonthCount { get; set; }

    public decimal PreviousIncome { get; set; }
    public decimal PreviousExpenses { get; set; }
    public decimal PreviousInvestments { get; set; }

    public int PreviousMonth { get; set; }
    public int PreviousYear { get; set; }
    public int NextMonth { get; set; }
    public int NextYear { get; set; }
    public bool CanGoToNext { get; set; }

    public decimal VariableExpenses => TotalExpenses - RecurringExpenses;
    public decimal VariableInvestments => TotalInvestments - RecurringInvestments;

    // Aporte sai do caixa igual a despesa, então a decomposição do mês olha a saída inteira.
    // Custo e aporte ficam em linhas separadas, senão a mesma barra somaria gasto com dinheiro
    // guardado e o card diria que o mês foi caro quando na verdade o mês foi de poupança.
    public decimal TotalOutflow => TotalExpenses + TotalInvestments;

    // Só o pedaço recorrente compromete a renda. Aporte extra é decisão daquele mês.
    public decimal RecurringCommitment => RecurringExpenses + RecurringInvestments;

    public decimal OutflowShare(decimal value) => TotalOutflow > 0 ? value / TotalOutflow * 100 : 0;

    public decimal RankingTotal => ExpensesByCategory.Sum(category => category.Total);

    public decimal? AverageInvestedPerMonth => InvestedMonthCount > 0
        ? TotalInvested / InvestedMonthCount
        : null;

    public decimal? IncomeChange => Variation(TotalIncome, PreviousIncome);
    public decimal? ExpensesChange => Variation(TotalExpenses, PreviousExpenses);
    public decimal? InvestmentsChange => Variation(TotalInvestments, PreviousInvestments);

    // Sem base anterior não existe variação a mostrar. Zero para zero também não é
    // queda de 100%, é ausência de dado.
    private static decimal? Variation(decimal current, decimal previous)
        => previous > 0 ? (current - previous) / previous * 100 : null;
}
