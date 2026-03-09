using Lawllit.Data.Finance.Entities;

namespace Lawllit.Web.Areas.Finance.Models;

public class CategoryListViewModel
{
    public List<Category> Categories { get; set; } = [];
    public string? FilterType { get; set; }
    public string? FilterSearch { get; set; }
}
