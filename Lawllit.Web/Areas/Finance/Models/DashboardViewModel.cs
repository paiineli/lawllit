using Lawllit.Data.Finance.Entities;

namespace Lawllit.Web.Areas.Finance.Models;

public class DashboardViewModel
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public decimal UpcomingExpenses { get; set; }
    public List<CategorySummary> ExpensesByCategory { get; set; } = [];
    public List<MonthlyTrend> MonthlyTrend { get; set; } = [];

    public bool IsCurrentMonth { get; set; }
    public int DaysInMonth { get; set; }
    public int DaysDone { get; set; }
    public decimal DailyAverage { get; set; }
    public decimal? MonthlyProjection { get; set; }

    public int PreviousMonth { get; set; }
    public int PreviousYear { get; set; }
    public int NextMonth { get; set; }
    public int NextYear { get; set; }
    public bool CanGoToNextMonth { get; set; }

    public decimal RankingTotal => ExpensesByCategory.Sum(category => category.Total);
}
