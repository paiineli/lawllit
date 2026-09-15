using Lawllit.Model.Common.Enums;
using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

[Authorize]
public class PainelController(IPainelRepository painelRepositorio) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(
        PeriodoPainelEnum periodo,
        int? mes,
        int? ano,
        CancellationToken cancellationToken)
        => View(await painelRepositorio.Montar(periodo, mes, ano, cancellationToken));
}
