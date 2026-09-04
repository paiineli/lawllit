using Lawllit.Site.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Site.Areas.Finance.Controllers;

[Area("Finance")]
public class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
        => View("Error", new ErrorViewMOD(HttpContext.TraceIdentifier));
}
