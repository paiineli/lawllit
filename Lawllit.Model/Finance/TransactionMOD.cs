using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance;

public sealed class TransactionMOD
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionTypeEnum Type { get; set; }
    public DateTime Date { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public CategoryMOD? Category { get; set; }
}
