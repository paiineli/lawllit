using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;

namespace Lawllit.Api.Finance.Services;

public sealed class CategoryService(ICategoryREP categoryRepository) : ICategoryService
{
    public Task<List<CategoryMOD>> GetFilteredAsync(Guid userId, CategoryFilterMOD filter, CancellationToken cancellationToken)
        => categoryRepository.GetFilteredAsync(userId, filter, cancellationToken);

    public Task<CategoryMOD?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        => categoryRepository.GetByIdAsync(userId, id, cancellationToken);

    public async Task<Result> CreateAsync(Guid userId, CategorySaveMOD category, CancellationToken cancellationToken)
    {
        var name = category.Name.Trim();

        if (await categoryRepository.ExistsAsync(userId, name, category.Type, excludeId: null, cancellationToken))
            return Result.Failure("Msg_CatAlreadyExists");

        await categoryRepository.AddAsync(new CategoryMOD
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = category.Type,
            UserId = userId,
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> EditAsync(Guid userId, CategorySaveMOD category, CancellationToken cancellationToken)
    {
        var existing = await categoryRepository.GetByIdAsync(userId, category.Id, cancellationToken);
        if (existing is null)
            return Result.Failure("Msg_CatNotFound");

        var name = category.Name.Trim();

        if (await categoryRepository.ExistsAsync(userId, name, category.Type, excludeId: category.Id, cancellationToken))
            return Result.Failure("Msg_CatAlreadyExists");

        existing.Name = name;
        existing.Type = category.Type;
        await categoryRepository.UpdateAsync(existing, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(userId, id, cancellationToken);
        if (category is null)
            return Result.Failure("Msg_CatNotFound");

        if (await categoryRepository.HasTransactionsAsync(userId, id, cancellationToken))
            return Result.Failure("Msg_CatHasTransactions");

        await categoryRepository.DeleteAsync(userId, id, cancellationToken);
        return Result.Success();
    }
}

#region Interfaces

public interface ICategoryService
{
    Task<List<CategoryMOD>> GetFilteredAsync(Guid userId, CategoryFilterMOD filter, CancellationToken cancellationToken);
    Task<CategoryMOD?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<Result> CreateAsync(Guid userId, CategorySaveMOD category, CancellationToken cancellationToken);
    Task<Result> EditAsync(Guid userId, CategorySaveMOD category, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}

#endregion
