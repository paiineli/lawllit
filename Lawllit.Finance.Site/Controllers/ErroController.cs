using Lawllit.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

[AllowAnonymous]
public class ErroController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
        => View("Erro", new ErroViewModel(HttpContext.TraceIdentifier));
}
