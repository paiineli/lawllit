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
public class TransactionController(
    ITransactionREP transactionRepository,
    ICategoryREP categoryRepository,
    IStringLocalizer<SharedResource> localizer) : BaseController
{
    private const int ItemsPerPage = 50;

    [HttpGet]
    public async Task<IActionResult> Index(
        TransactionTypeEnum? type,
        int? month,
        int? year,
        string? search,
        int page,
        CancellationToken cancellationToken)
    {
        var filter = new TransactionFilterMOD
        {
            Type = type,
            Month = month,
            Year = year,
            Search = search,
            Pagination = new Pagination { CurrentPage = Math.Max(page, 1), PageSize = ItemsPerPage },
        };

        var transactionPage = await transactionRepository.GetPageAsync(filter, cancellationToken);
        var categories = await categoryRepository.GetFilteredAsync(new CategoryFilterMOD(), cancellationToken);

        return View(new TransactionListViewMOD
        {
            Page = transactionPage.Page,
            Categories = categories,
            FilterType = type,
            FilterSearch = search,
            FilterMonth = transactionPage.Month,
            FilterYear = transactionPage.Year,
            TotalIncome = transactionPage.TotalIncome,
            TotalExpenses = transactionPage.TotalExpenses,
            TotalInvestments = transactionPage.TotalInvestments,
            PendingRecurringCount = transactionPage.PendingRecurringCount,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionFormViewMOD transactionForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError("Msg_DataInvalid");

        var result = await transactionRepository.CreateAsync(ToSaveMOD(transactionForm), cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_TransCreated")
            : RedirectWithError(result.ErrorKey!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TransactionFormViewMOD transactionForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError("Msg_DataInvalid");

        transactionForm.Id = id;
        var result = await transactionRepository.EditAsync(ToSaveMOD(transactionForm), cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_TransUpdated")
            : RedirectWithError(result.ErrorKey!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await transactionRepository.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_TransDeleted")
            : RedirectWithError(result.ErrorKey!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportRecurring(int month, int year, CancellationToken cancellationToken)
    {
        var importedCount = await transactionRepository.ImportRecurringAsync(
            new ImportRecurringMOD { Month = month, Year = year },
            cancellationToken);

        TempData["Success"] = string.Format(localizer["Msg_RecurringImported"].Value, importedCount);
        return RedirectToAction("Index");
    }

    private static TransactionSaveMOD ToSaveMOD(TransactionFormViewMOD form) => new()
    {
        Id = form.Id,
        Description = form.Description,
        Amount = form.Amount,
        Type = form.Type,
        Date = form.Date,
        CategoryId = form.CategoryId,
        IsRecurring = form.IsRecurring,
    };

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
