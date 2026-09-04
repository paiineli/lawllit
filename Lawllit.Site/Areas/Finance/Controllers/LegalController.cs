using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Site.Areas.Finance.Controllers;

[Area("Finance")]
public class LegalController : Controller
{
    [HttpGet]
    public IActionResult Privacy() => View();

    [HttpGet]
    public IActionResult Terms() => View();
}
