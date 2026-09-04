using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance.Contracts;

public sealed class TransactionFilterMOD
{
    public TransactionTypeEnum? Type { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? Search { get; set; }
    public Pagination Pagination { get; set; } = new();
}

// Os totais vêm do banco sobre o mês inteiro, não sobre a página corrente,
// senão o rodapé mudaria de valor a cada troca de página.
public sealed class TransactionPageMOD
{
    public PagedResult<TransactionMOD> Page { get; set; } = new();
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalInvestments { get; set; }
    public int PendingRecurringCount { get; set; }
}

public sealed class TransactionSaveMOD
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public TransactionTypeEnum Type { get; set; }
    public DateTime Date { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsRecurring { get; set; }
}

public sealed class ImportRecurringMOD
{
    public int Month { get; set; }
    public int Year { get; set; }
}

public sealed class ImportRecurringResultMOD
{
    public int ImportedCount { get; set; }
}
