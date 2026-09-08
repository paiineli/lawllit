using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;

namespace Lawllit.Api.Finance.Services;

public sealed class DashboardService(ITransactionREP transactionRepository) : IDashboardService
{
    private const int MonthTrendCount = 6;
    private const int YearTrendCount = 12;

    // Um mês sozinho é ruído e seis demora para reagir, então três meses é a janela
    // de referência do modo mês. No modo ano a referência natural é o ano anterior.
    private const int MonthReferenceCount = 3;
    private const int YearReferenceCount = 12;

    public async Task<DashboardMOD> BuildAsync(
        Guid userId,
        DashboardPeriodEnum period,
        int? requestedMonth,
        int? requestedYear,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var selectedYear = requestedYear ?? now.Year;
        var selectedMonth = period == DashboardPeriodEnum.YEAR ? 1 : requestedMonth ?? now.Month;

        var from = new DateTime(selectedYear, selectedMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = period == DashboardPeriodEnum.YEAR ? from.AddYears(1) : from.AddMonths(1);

        // O período anterior tem o mesmo tamanho e encosta no atual, o que deixa a
        // consulta de totais resolver os dois de uma vez.
        var previousFrom = period == DashboardPeriodEnum.YEAR ? from.AddYears(-1) : from.AddMonths(-1);

        var referenceMonths = period == DashboardPeriodEnum.YEAR ? YearReferenceCount : MonthReferenceCount;
        var referenceFrom = from.AddMonths(-referenceMonths);

        var trendCount = period == DashboardPeriodEnum.YEAR ? YearTrendCount : MonthTrendCount;
        var trendToMonth = period == DashboardPeriodEnum.YEAR ? 12 : selectedMonth;

        var totals = await transactionRepository.GetPeriodTotalsAsync(userId, from, to, previousFrom, cancellationToken);
        var categorySpend = await transactionRepository.GetCategorySpendAsync(userId, from, to, referenceFrom, referenceMonths, cancellationToken);
        var trend = await transactionRepository.GetMonthlyTrendAsync(userId, trendToMonth, selectedYear, trendCount, cancellationToken);
        var invested = await transactionRepository.GetInvestedTotalAsync(userId, to, cancellationToken);

        var isCurrentMonth = period == DashboardPeriodEnum.MONTH && selectedMonth == now.Month && selectedYear == now.Year;

        // Vencimento a vencer só faz sentido olhando um mês. No ano a informação seria
        // "o resto do ano", que não ajuda a decidir nada.
        var upcomingExpenses = period == DashboardPeriodEnum.MONTH
            ? await transactionRepository.GetUpcomingExpensesAsync(userId, from, to, cancellationToken)
            : 0m;

        var dashboard = new DashboardMOD
        {
            Period = period,
            Month = period == DashboardPeriodEnum.YEAR ? 0 : selectedMonth,
            Year = selectedYear,
            TotalIncome = totals.TotalIncome,
            TotalExpenses = totals.TotalExpenses,
            TotalInvestments = totals.TotalInvestments,
            Balance = totals.Balance,
            RecurringExpenses = totals.RecurringExpenses,
            RecurringInvestments = totals.RecurringInvestments,
            PreviousIncome = totals.PreviousIncome,
            PreviousExpenses = totals.PreviousExpenses,
            PreviousInvestments = totals.PreviousInvestments,
            ExpensesByCategory = BuildComparisons(categorySpend),
            ReferenceMonthCount = referenceMonths,
            MonthlyTrend = trend,
            UpcomingExpenses = upcomingExpenses,
            TotalInvested = invested.Total,
            InvestedMonthCount = invested.MonthCount,
            IsCurrentMonth = isCurrentMonth,
        };

        ApplySavingsRate(dashboard);
        ApplyPace(dashboard, period, selectedMonth, selectedYear, now, isCurrentMonth);
        ApplyNavigation(dashboard, period, from, now);

        return dashboard;
    }

    private static List<CategoryComparisonMOD> BuildComparisons(List<CategorySpendMOD> categorySpend)
        => categorySpend
            .Select(category => new CategoryComparisonMOD(
                category.CategoryName,
                category.Total,
                category.ReferenceAverage,
                category.ReferenceAverage > 0
                    ? (category.Total - category.ReferenceAverage) / category.ReferenceAverage * 100
                    : null))
            .ToList();

    // Poupança e saldo respondem perguntas diferentes. O saldo já desconta o aporte,
    // porque ele sai do caixa. A taxa de poupança não desconta, porque dinheiro
    // investido foi guardado e não consumido.
    private static void ApplySavingsRate(DashboardMOD dashboard)
    {
        if (dashboard.TotalIncome > 0)
            dashboard.SavingsRate = (dashboard.TotalIncome - dashboard.TotalExpenses) / dashboard.TotalIncome * 100;

        // Aporte marcado como recorrente é compromisso do mês igual a uma despesa fixa, o
        // recorrente já é importado mês a mês. Aporte esporádico fica fora, é escolha do mês.
        if (dashboard.TotalIncome > 0)
            dashboard.CommittedShare = dashboard.RecurringCommitment / dashboard.TotalIncome * 100;
    }

    private static void ApplyPace(
        DashboardMOD dashboard,
        DashboardPeriodEnum period,
        int selectedMonth,
        int selectedYear,
        DateTime now,
        bool isCurrentMonth)
    {
        if (period == DashboardPeriodEnum.YEAR) return;

        var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
        var daysDone = isCurrentMonth ? now.Day : daysInMonth;

        // No mês corrente a média diária só conta o que já venceu, senão despesa
        // agendada para o fim do mês inflaria a média e a projeção.
        var pastExpenses = isCurrentMonth
            ? dashboard.TotalExpenses - dashboard.UpcomingExpenses
            : dashboard.TotalExpenses;

        dashboard.DaysInMonth = daysInMonth;
        dashboard.DaysDone = daysDone;
        dashboard.DailyAverage = pastExpenses > 0 ? pastExpenses / daysDone : 0;

        dashboard.MonthlyProjection = isCurrentMonth && pastExpenses > 0
            ? pastExpenses / daysDone * daysInMonth
            : null;
    }

    // A navegação para frente ficava travada no mês corrente, o que escondia lançamento
    // futuro que a tela de transações deixa cadastrar. Liberada, com teto de um ano
    // para não virar rolagem infinita em período vazio.
    private static void ApplyNavigation(DashboardMOD dashboard, DashboardPeriodEnum period, DateTime from, DateTime now)
    {
        var previous = period == DashboardPeriodEnum.YEAR ? from.AddYears(-1) : from.AddMonths(-1);
        var next = period == DashboardPeriodEnum.YEAR ? from.AddYears(1) : from.AddMonths(1);

        dashboard.PreviousMonth = previous.Month;
        dashboard.PreviousYear = previous.Year;
        dashboard.NextMonth = next.Month;
        dashboard.NextYear = next.Year;

        var forwardLimit = period == DashboardPeriodEnum.YEAR
            ? new DateTime(now.Year + 1, 1, 1)
            : new DateTime(now.Year, now.Month, 1).AddMonths(12);

        dashboard.CanGoToNext = next.Date <= forwardLimit.Date;
    }
}

#region Interfaces

public interface IDashboardService
{
    Task<DashboardMOD> BuildAsync(
        Guid userId,
        DashboardPeriodEnum period,
        int? requestedMonth,
        int? requestedYear,
        CancellationToken cancellationToken);
}

#endregion
