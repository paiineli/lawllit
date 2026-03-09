using Lawllit.Data.Finance.Entities;
using System.ComponentModel.DataAnnotations;

namespace Lawllit.Web.Areas.Finance.Models;

public class TransactionFormViewModel
{
    public Guid? Id { get; set; }

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
