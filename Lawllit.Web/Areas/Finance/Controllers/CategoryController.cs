using Lawllit.Api;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Lawllit.Web.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class CategoryController(ICategoryService categoryService, IStringLocalizer<SharedResource> localizer) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, string? search)
    {
        var viewModel = await categoryService.GetListViewModelAsync(GetUserId(), type, search);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel categoryForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = localizer["Msg_CatNameRequired"].Value;
            return RedirectToAction("Index");
        }

        var result = await categoryService.CreateAsync(GetUserId(), categoryForm);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_CatCreated"].Value;
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryFormViewModel categoryForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = localizer["Msg_CatNameRequired"].Value;
            return RedirectToAction("Index");
        }

        var result = await categoryService.EditAsync(GetUserId(), id, categoryForm);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_CatUpdated"].Value;
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await categoryService.DeleteAsync(GetUserId(), id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_CatDeleted"].Value;
        return RedirectToAction("Index");
    }
}
