using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance.Contracts;

public sealed class CategoryFilterMOD
{
    public TransactionTypeEnum? Type { get; set; }
    public string? Search { get; set; }
}

public sealed class CategorySaveMOD
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionTypeEnum Type { get; set; }
}
