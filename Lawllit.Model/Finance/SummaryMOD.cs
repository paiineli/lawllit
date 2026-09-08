namespace Lawllit.Model.Finance;

// Gasto da categoria no período ao lado da média mensal dela nos meses anteriores.
// A referência sai do histórico, então o usuário não cadastra teto nenhum.
public sealed record CategorySpendMOD(string CategoryName, decimal Total, decimal ReferenceAverage);

// O mesmo par acima já com a variação calculada. Nulo quando não há histórico com
// que comparar, porque zero contra zero é ausência de dado e não queda.
public sealed record CategoryComparisonMOD(
    string CategoryName,
    decimal Total,
    decimal ReferenceAverage,
    decimal? VariationPercent);

public sealed record MonthlyTrendMOD(int Month, int Year, decimal Income, decimal Expenses, decimal Investments);

// Totais do período em tela e do período imediatamente anterior, no mesmo registro,
// porque uma consulta só resolve os dois.
public sealed record PeriodTotalsMOD(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal TotalInvestments,
    decimal RecurringExpenses,
    decimal RecurringInvestments,
    decimal PreviousIncome,
    decimal PreviousExpenses,
    decimal PreviousInvestments
)
{
    // Investimento sai do caixa do período igual a uma despesa, então abate do saldo.
    public decimal Balance => TotalIncome - TotalExpenses - TotalInvestments;
}

public sealed record InvestedTotalMOD(decimal Total, int MonthCount);
