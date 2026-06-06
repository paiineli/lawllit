using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Web.Areas.Finance.Controllers;

[Area("Finance")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View();
    }
}
