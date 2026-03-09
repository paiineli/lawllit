using Lawllit.Web.Areas.Finance.Models;

namespace Lawllit.Web.Areas.Finance.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryListViewModel> GetListViewModelAsync(Guid userId, string? type, string? search);
    Task<Result> CreateAsync(Guid userId, CategoryFormViewModel form);
    Task<Result> EditAsync(Guid userId, Guid id, CategoryFormViewModel form);
    Task<Result> DeleteAsync(Guid userId, Guid id);
}
