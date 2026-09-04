using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using System.ComponentModel.DataAnnotations;

namespace Lawllit.Site.Models.Finance;

public sealed class TransactionListViewMOD
{
    public PagedResult<TransactionMOD> Page { get; set; } = new();
    public List<CategoryMOD> Categories { get; set; } = [];
    public TransactionTypeEnum? FilterType { get; set; }
    public string? FilterSearch { get; set; }
    public int FilterMonth { get; set; }
    public int FilterYear { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalInvestments { get; set; }
    public int PendingRecurringCount { get; set; }

    public List<TransactionMOD> Transactions => Page.Items;

    public bool HasActiveFilters => FilterType.HasValue || !string.IsNullOrEmpty(FilterSearch);
}

public sealed class TransactionFormViewMOD
{
    public Guid Id { get; set; }

    [StringLength(200, ErrorMessage = "Val_DescriptionMaxLength")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Val_AmountRange")]
    public decimal Amount { get; set; }

    public TransactionTypeEnum Type { get; set; } = TransactionTypeEnum.EXPENSE;

    [Required(ErrorMessage = "Val_DateRequired")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Val_CategoryRequired")]
    public Guid CategoryId { get; set; }

    public bool IsRecurring { get; set; }
}
