using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Finance.Contracts;

namespace Lawllit.Api.Finance.Services;

public sealed class DashboardService(ITransactionREP transactionRepository) : IDashboardService
{
    private const int TrendMonthCount = 6;

    public async Task<DashboardMOD> BuildAsync(Guid userId, int? requestedMonth, int? requestedYear, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var selectedMonth = requestedMonth ?? now.Month;
        var selectedYear = requestedYear ?? now.Year;

        var summary = await transactionRepository.GetSummaryAsync(userId, selectedMonth, selectedYear, cancellationToken);
        var monthlyTrend = await transactionRepository.GetMonthlyTrendAsync(userId, selectedMonth, selectedYear, TrendMonthCount, cancellationToken);
        var upcomingExpenses = await transactionRepository.GetUpcomingExpensesAsync(userId, selectedMonth, selectedYear, cancellationToken);

        var isCurrentMonth = selectedMonth == now.Month && selectedYear == now.Year;
        var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
        var daysDone = isCurrentMonth ? now.Day : daysInMonth;

        // No mês corrente a média diária só conta o que já venceu, senão despesa
        // agendada para o fim do mês inflaria a média e a projeção.
        var pastExpenses = isCurrentMonth ? summary.TotalExpenses - upcomingExpenses : summary.TotalExpenses;
        var dailyAverage = pastExpenses > 0 ? pastExpenses / daysDone : 0;

        var monthlyProjection = isCurrentMonth && pastExpenses > 0
            ? pastExpenses / daysDone * daysInMonth
            : (decimal?)null;

        var previousMonthDate = new DateTime(selectedYear, selectedMonth, 1).AddMonths(-1);
        var nextMonthDate = new DateTime(selectedYear, selectedMonth, 1).AddMonths(1);

        return new DashboardMOD
        {
            Month = selectedMonth,
            Year = selectedYear,
            TotalIncome = summary.TotalIncome,
            TotalExpenses = summary.TotalExpenses,
            TotalInvestments = summary.TotalInvestments,
            Balance = summary.Balance,
            ExpensesByCategory = summary.ExpensesByCategory,
            MonthlyTrend = monthlyTrend,
            UpcomingExpenses = upcomingExpenses,
            IsCurrentMonth = isCurrentMonth,
            DaysInMonth = daysInMonth,
            DaysDone = daysDone,
            DailyAverage = dailyAverage,
            MonthlyProjection = monthlyProjection,
            PreviousMonth = previousMonthDate.Month,
            PreviousYear = previousMonthDate.Year,
            NextMonth = nextMonthDate.Month,
            NextYear = nextMonthDate.Year,
            CanGoToNextMonth = !isCurrentMonth,
        };
    }
}

#region Interfaces

public interface IDashboardService
{
    Task<DashboardMOD> BuildAsync(Guid userId, int? requestedMonth, int? requestedYear, CancellationToken cancellationToken);
}

#endregion
