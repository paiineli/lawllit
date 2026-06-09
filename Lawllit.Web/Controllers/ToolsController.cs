using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Lawllit.Web.Controllers;

public class ToolsController : Controller
{
    public IActionResult Index() => View();

    public IActionResult SpreadsheetMerger() => View();

    public IActionResult PdfMerger() => View();

    public IActionResult Base64() => View();

    public IActionResult TextCase() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public IActionResult PdfMerger(IFormFileCollection files)
    {
        if (files is null || files.Count < 2)
        {
            ViewBag.ErrorKey = "PdfMerger_Error_MinFiles";
            return View();
        }

        if (files.Any(f => !Path.GetExtension(f.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase)))
        {
            ViewBag.ErrorKey = "PdfMerger_Error_InvalidFormat";
            return View();
        }

        try
        {
            using var output = new PdfDocument();
            foreach (var file in files)
            {
                using var stream = file.OpenReadStream();
                using var input = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                for (var i = 0; i < input.PageCount; i++)
                    output.AddPage(input.Pages[i]);
            }
            using var ms = new MemoryStream();
            output.Save(ms);
            return File(ms.ToArray(), "application/pdf", "merged.pdf");
        }
        catch
        {
            ViewBag.ErrorKey = "PdfMerger_Error_Processing";
            return View();
        }
    }
}
