using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services;

public class DashboardService(ITransactionRepository transactionRepository) : IDashboardService
{
    public async Task<DashboardViewModel> BuildDashboardAsync(Guid userId, int? requestedMonth, int? requestedYear)
    {
        var now = DateTime.Now;
        var selectedMonth = requestedMonth ?? now.Month;
        var selectedYear = requestedYear ?? now.Year;

        var summary = await transactionRepository.GetSummaryAsync(userId, selectedMonth, selectedYear);
        var monthlyTrend = await transactionRepository.GetMonthlyTrendAsync(userId, selectedMonth, selectedYear);
        var upcomingExpenses = await transactionRepository.GetUpcomingExpensesAsync(userId, selectedMonth, selectedYear);

        bool isCurrentMonth = selectedMonth == now.Month && selectedYear == now.Year;
        int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
        int daysDone = isCurrentMonth ? now.Day : daysInMonth;

        decimal pastExpenses = isCurrentMonth ? summary.TotalExpenses - upcomingExpenses : summary.TotalExpenses;
        decimal dailyAverage = pastExpenses > 0 ? pastExpenses / daysDone : 0;
        decimal? monthlyProjection = isCurrentMonth && pastExpenses > 0
            ? (pastExpenses / daysDone) * daysInMonth
            : null;

        int previousMonth = selectedMonth == 1 ? 12 : selectedMonth - 1;
        int previousYear = selectedMonth == 1 ? selectedYear - 1 : selectedYear;
        int nextMonth = selectedMonth == 12 ? 1 : selectedMonth + 1;
        int nextYear = selectedMonth == 12 ? selectedYear + 1 : selectedYear;

        return new DashboardViewModel
        {
            Month = selectedMonth,
            Year = selectedYear,
            TotalIncome = summary.TotalIncome,
            TotalExpenses = summary.TotalExpenses,
            Balance = summary.Balance,
            ExpensesByCategory = summary.ExpensesByCategory,
            MonthlyTrend = monthlyTrend,
            UpcomingExpenses = upcomingExpenses,
            IsCurrentMonth = isCurrentMonth,
            DaysInMonth = daysInMonth,
            DaysDone = daysDone,
            DailyAverage = dailyAverage,
            MonthlyProjection = monthlyProjection,
            PreviousMonth = previousMonth,
            PreviousYear = previousYear,
            NextMonth = nextMonth,
            NextYear = nextYear,
            CanGoToNextMonth = !isCurrentMonth,
        };
    }
}
