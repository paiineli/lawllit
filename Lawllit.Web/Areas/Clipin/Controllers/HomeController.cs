using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Web.Areas.Clipin.Controllers;

[Area("Clipin")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
