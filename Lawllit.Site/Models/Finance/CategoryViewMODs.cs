using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using System.ComponentModel.DataAnnotations;

namespace Lawllit.Site.Models.Finance;

public sealed class CategoryListViewMOD
{
    public List<CategoryMOD> Categories { get; set; } = [];
    public TransactionTypeEnum? FilterType { get; set; }
    public string? FilterSearch { get; set; }
}

public sealed class CategoryFormViewMOD
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Val_CatNameRequired")]
    [StringLength(100, ErrorMessage = "Val_CatNameMaxLength")]
    public string Name { get; set; } = string.Empty;

    public TransactionTypeEnum Type { get; set; } = TransactionTypeEnum.EXPENSE;
}
