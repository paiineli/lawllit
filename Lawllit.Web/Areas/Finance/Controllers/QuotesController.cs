using Lawllit.Api;
using Lawllit.Api.Finance.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Lawllit.Web.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class QuotesController(IQuotesService quotesService, IStringLocalizer<SharedResource> localizer, ILogger<QuotesController> logger) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (quotes, errorTechDetails) = await quotesService.GetQuotesAsync();

        if (errorTechDetails is not null)
        {
            logger.LogError("Quotes fetch failed: {Error}", errorTechDetails);
            TempData["Error"] = localizer["Quote_LoadError"].Value;
        }

        return View(quotes);
    }
}
