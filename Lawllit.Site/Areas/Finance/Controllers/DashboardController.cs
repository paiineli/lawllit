using Lawllit.Model.Common.Enums;
using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Site.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class DashboardController(IDashboardREP dashboardRepository) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(
        DashboardPeriodEnum period,
        int? month,
        int? year,
        CancellationToken cancellationToken)
        => View(await dashboardRepository.BuildAsync(period, month, year, cancellationToken));
}
