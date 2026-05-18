using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance;
using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<CategoryListViewModel> GetListViewModelAsync(Guid userId, string? type, string? search)
    {
        var categories = await categoryRepository.GetAllByUserAsync(userId);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, out var parsedType))
            categories = categories.Where(category => category.Type == parsedType).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            categories = categories
                .Where(category => category.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        return new CategoryListViewModel
        {
            Categories = categories,
            FilterType = type,
            FilterSearch = search,
        };
    }

    public async Task<Result> CreateAsync(Guid userId, CategoryFormViewModel form)
    {
        if (await categoryRepository.ExistsAsync(userId, form.Name, form.Type))
            return Result.Failure("Msg_CatAlreadyExists");

        await categoryRepository.AddAsync(new Category
        {
            Id = Guid.NewGuid(),
            Name = form.Name.Trim(),
            Type = form.Type,
            UserId = userId,
        });

        return Result.Success();
    }

    public async Task<Result> EditAsync(Guid userId, Guid id, CategoryFormViewModel form)
    {
        var category = await categoryRepository.GetByIdAsync(userId, id);
        if (category is null)
            return Result.Failure("Msg_CatNotFound");

        if (await categoryRepository.ExistsAsync(userId, form.Name, form.Type, excludeId: id))
            return Result.Failure("Msg_CatAlreadyExists");

        category.Name = form.Name.Trim();
        category.Type = form.Type;
        await categoryRepository.UpdateAsync(category);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id)
    {
        var category = await categoryRepository.GetByIdAsync(userId, id);
        if (category is null)
            return Result.Failure("Msg_CatNotFound");

        if (await categoryRepository.HasTransactionsAsync(userId, id))
            return Result.Failure("Msg_CatHasTransactions");

        await categoryRepository.DeleteAsync(id);
        return Result.Success();
    }
}
