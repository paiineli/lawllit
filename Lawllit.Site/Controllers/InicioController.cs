using Lawllit.Site.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Site.Controllers;

public class InicioController : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Erro() => View(new ErroViewModel(HttpContext.TraceIdentifier));
}
