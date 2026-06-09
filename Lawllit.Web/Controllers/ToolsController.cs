using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Web.Controllers;

public class ToolsController : Controller
{
    public IActionResult Index() => View();

    public IActionResult SpreadsheetMerger() => View();

    public IActionResult PdfMerger() => View();

    public IActionResult Base64() => View();

    public IActionResult TextCase() => View();
}
