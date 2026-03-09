namespace Lawllit.Data.Finance.Entities;

public record CategorySummary(string CategoryName, decimal Total);

public record MonthlyTrend(int Month, int Year, decimal Income, decimal Expenses);

public record MonthlySummary(
    int Month,
    int Year,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    List<CategorySummary> ExpensesByCategory
);
