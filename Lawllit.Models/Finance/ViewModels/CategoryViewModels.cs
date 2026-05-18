using System.ComponentModel.DataAnnotations;

namespace Lawllit.Models.Finance.ViewModels;

public class CategoryListViewModel
{
    public List<Category> Categories { get; set; } = [];
    public string? FilterType { get; set; }
    public string? FilterSearch { get; set; }
}

public class CategoryFormViewModel
{
    [Required(ErrorMessage = "Val_CatNameRequired")]
    [StringLength(100, ErrorMessage = "Val_CatNameMaxLength")]
    public string Name { get; set; } = string.Empty;

    public TransactionType Type { get; set; } = TransactionType.Expense;
}
