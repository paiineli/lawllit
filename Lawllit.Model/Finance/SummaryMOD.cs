namespace Lawllit.Model.Finance;

public sealed record CategorySummaryMOD(string CategoryName, decimal Total);

public sealed record MonthlyTrendMOD(int Month, int Year, decimal Income, decimal Expenses, decimal Investments);

public sealed record MonthlySummaryMOD(
    int Month,
    int Year,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal TotalInvestments,
    decimal Balance,
    List<CategorySummaryMOD> ExpensesByCategory
);
