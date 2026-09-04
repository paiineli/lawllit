using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance;

public sealed class CategoryMOD
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionTypeEnum Type { get; set; }
    public Guid UserId { get; set; }
}
