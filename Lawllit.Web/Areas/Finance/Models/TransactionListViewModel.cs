using Lawllit.Data.Finance.Entities;

namespace Lawllit.Web.Areas.Finance.Models;

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
