using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Finance;
using Lawllit.Site.Common;
using Lawllit.Site.Models.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text;

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

    [HttpGet]
    public async Task<IActionResult> Export(
        TransactionTypeEnum? type,
        int? month,
        int? year,
        string? search,
        CancellationToken cancellationToken)
    {
        var filter = new TransactionFilterMOD { Type = type, Month = month, Year = year, Search = search };
        var transactions = await transactionRepository.GetAllFilteredAsync(filter, cancellationToken);

        var fileName = $"lawllit-{year ?? DateTime.Now.Year}-{(month ?? DateTime.Now.Month):00}.csv";

        return File(BuildCsv(transactions), "text/csv", fileName);
    }

    // Ponto e vírgula como separador e vírgula decimal, que é o que o Excel em pt-BR
    // espera. Com vírgula de separador ele joga tudo numa coluna só. O BOM na frente
    // evita acento quebrado ao abrir o arquivo no Excel.
    private byte[] BuildCsv(List<TransactionMOD> transactions)
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        var csv = new StringBuilder();

        csv.Append('﻿');
        csv.AppendLine(string.Join(';',
            localizer["Lbl_Date"].Value,
            localizer["Lbl_Description"].Value,
            localizer["Lbl_Category"].Value,
            localizer["Lbl_Type"].Value,
            localizer["Trans_IsRecurring"].Value,
            localizer["Lbl_Amount"].Value));

        foreach (var transaction in transactions)
        {
            csv.AppendLine(string.Join(';',
                transaction.Date.ToString("dd/MM/yyyy", culture),
                Escape(transaction.Description),
                Escape(transaction.Category?.Name ?? string.Empty),
                localizer[transaction.Type.LabelKey()].Value,
                transaction.IsRecurring ? localizer["Lbl_Yes"].Value : localizer["Lbl_No"].Value,
                transaction.Amount.ToString("F2", culture)));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    // Campo com ponto e vírgula, aspas ou quebra de linha vira campo entre aspas,
    // e aspas interna dobra, conforme o RFC do CSV.
    private static string Escape(string value)
    {
        if (!value.Contains(';') && !value.Contains('"') && !value.Contains('\n'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        TransactionFormViewMOD transactionForm,
        [Bind(Prefix = "filter")] TransactionFilterRouteViewMOD filter,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError("Msg_DataInvalid", filter);

        var result = await transactionRepository.CreateAsync(ToSaveMOD(transactionForm), cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_TransCreated", filter)
            : RedirectWithError(result.ErrorKey!, filter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        TransactionFormViewMOD transactionForm,
        [Bind(Prefix = "filter")] TransactionFilterRouteViewMOD filter,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError("Msg_DataInvalid", filter);

        transactionForm.Id = id;
        var result = await transactionRepository.EditAsync(ToSaveMOD(transactionForm), cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_TransUpdated", filter)
            : RedirectWithError(result.ErrorKey!, filter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        Guid id,
        [Bind(Prefix = "filter")] TransactionFilterRouteViewMOD filter,
        CancellationToken cancellationToken)
    {
        var result = await transactionRepository.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_TransDeleted", filter)
            : RedirectWithError(result.ErrorKey!, filter);
    }

    // Importa a recorrência no mês que está sendo visto, que é o mesmo período do aviso de
    // pendência exibido na tela, e não no mês corrente do calendário.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportRecurring(
        [Bind(Prefix = "filter")] TransactionFilterRouteViewMOD filter,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var importedCount = await transactionRepository.ImportRecurringAsync(
            new ImportRecurringMOD { Month = filter.Month ?? now.Month, Year = filter.Year ?? now.Year },
            cancellationToken);

        TempData["Success"] = string.Format(localizer["Msg_RecurringImported"].Value, importedCount);
        return RedirectToAction("Index", filter.ToRouteValues());
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

    private IActionResult RedirectWithSuccess(string messageKey, TransactionFilterRouteViewMOD filter)
    {
        TempData["Success"] = localizer[messageKey].Value;
        return RedirectToAction("Index", filter.ToRouteValues());
    }

    private IActionResult RedirectWithError(string messageKey, TransactionFilterRouteViewMOD filter)
    {
        TempData["Error"] = localizer[messageKey].Value;
        return RedirectToAction("Index", filter.ToRouteValues());
    }
}
