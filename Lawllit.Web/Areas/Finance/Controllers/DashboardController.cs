using Lawllit.Web.Areas.Finance.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Web.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(int? month, int? year)
    {
        var viewModel = await dashboardService.BuildDashboardAsync(GetUserId(), month, year);
        return View(viewModel);
    }
}
