using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

[AllowAnonymous]
public class LegalController : Controller
{
    [HttpGet]
    public IActionResult Privacidade() => View();

    [HttpGet]
    public IActionResult Termos() => View();
}
