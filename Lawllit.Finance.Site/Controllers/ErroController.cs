using Lawllit.Finance.Site.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

public class ErroController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
        => View("Erro", new ErroViewModel(HttpContext.TraceIdentifier));
}
