using System.ComponentModel.DataAnnotations;

namespace Lawllit.Models.Finance.ViewModels;

public class TransactionListViewModel
{
    public List<Transaction> Transactions { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
    public string? FilterType { get; set; }
    public string? FilterSearch { get; set; }
    public int FilterMonth { get; set; }
    public int FilterYear { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public int PendingRecurringCount { get; set; }

    public bool HasActiveFilters => !string.IsNullOrEmpty(FilterType) || !string.IsNullOrEmpty(FilterSearch);
}

public class TransactionFormViewModel
{
    [StringLength(200, ErrorMessage = "Val_DescriptionMaxLength")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Val_AmountRange")]
    public decimal Amount { get; set; }

    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "Val_DateRequired")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Val_CategoryRequired")]
    public Guid CategoryId { get; set; }

    public bool IsRecurring { get; set; } = false;
}
