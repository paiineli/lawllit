using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Finance;
using Lawllit.Site.Models.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Lawllit.Site.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class CategoryController(ICategoryREP categoryRepository, IStringLocalizer<SharedResource> localizer) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(TransactionTypeEnum? type, string? search, CancellationToken cancellationToken)
    {
        var filter = new CategoryFilterMOD { Type = type, Search = search };
        var categories = await categoryRepository.GetFilteredAsync(filter, cancellationToken);

        return View(new CategoryListViewMOD
        {
            Categories = categories,
            FilterType = type,
            FilterSearch = search,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewMOD categoryForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError("Msg_CatNameRequired");

        var result = await categoryRepository.CreateAsync(
            new CategorySaveMOD { Name = categoryForm.Name, Type = categoryForm.Type },
            cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_CatCreated")
            : RedirectWithError(result.ErrorKey!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryFormViewMOD categoryForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError("Msg_CatNameRequired");

        var result = await categoryRepository.EditAsync(
            new CategorySaveMOD { Id = id, Name = categoryForm.Name, Type = categoryForm.Type },
            cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_CatUpdated")
            : RedirectWithError(result.ErrorKey!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await categoryRepository.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_CatDeleted")
            : RedirectWithError(result.ErrorKey!);
    }

    private IActionResult RedirectWithSuccess(string messageKey)
    {
        TempData["Success"] = localizer[messageKey].Value;
        return RedirectToAction("Index");
    }

    private IActionResult RedirectWithError(string messageKey)
    {
        TempData["Error"] = localizer[messageKey].Value;
        return RedirectToAction("Index");
    }
}
