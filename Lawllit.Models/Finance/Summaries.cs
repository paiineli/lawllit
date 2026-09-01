namespace Lawllit.Models.Finance;

public record CategorySummary(string CategoryName, decimal Total);

public record MonthlyTrend(int Month, int Year, decimal Income, decimal Expenses, decimal Investments);

public record MonthlySummary(
    int Month,
    int Year,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal TotalInvestments,
    decimal Balance,
    List<CategorySummary> ExpensesByCategory
);
