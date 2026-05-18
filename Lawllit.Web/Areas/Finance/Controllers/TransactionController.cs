using Lawllit.Api;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Lawllit.Web.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class TransactionController(ITransactionService transactionService, IStringLocalizer<SharedResource> localizer) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, int? month, int? year, string? search)
    {
        var viewModel = await transactionService.GetListViewModelAsync(GetUserId(), type, month, year, search);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionFormViewModel transactionForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = localizer["Msg_DataInvalid"].Value;
            return RedirectToAction("Index");
        }

        var result = await transactionService.CreateAsync(GetUserId(), transactionForm);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_TransCreated"].Value;
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TransactionFormViewModel transactionForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = localizer["Msg_DataInvalid"].Value;
            return RedirectToAction("Index");
        }

        var result = await transactionService.EditAsync(GetUserId(), id, transactionForm);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_TransUpdated"].Value;
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await transactionService.DeleteAsync(GetUserId(), id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index");
        }

        TempData["Success"] = localizer["Msg_TransDeleted"].Value;
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportRecurring(int month, int year)
    {
        var importedCount = await transactionService.ImportRecurringTransactionsAsync(GetUserId(), month, year);
        TempData["Success"] = string.Format(localizer["Msg_RecurringImported"].Value, importedCount);
        return RedirectToAction("Index");
    }
}
