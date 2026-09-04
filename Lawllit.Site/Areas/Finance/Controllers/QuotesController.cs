using Lawllit.Model.Common;
using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Lawllit.Site.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class QuotesController(IQuoteREP quoteRepository, IStringLocalizer<SharedResource> localizer) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await quoteRepository.GetQuotesAsync(cancellationToken);

        // A API já logou o motivo técnico da falha, aqui só sobra a mensagem ao usuário.
        if (!result.IsSuccess)
            TempData["Error"] = localizer[result.ErrorKey!].Value;

        return View(result.Value ?? []);
    }
}
