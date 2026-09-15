using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

public class InicioController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Painel");

        return View();
    }
}
