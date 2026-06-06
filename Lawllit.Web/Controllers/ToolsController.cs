using ClosedXML.Excel;
using ExcelDataReader;
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
    public IActionResult SpreadsheetMerger(IFormFileCollection files)
    {
        if (files is null || files.Count < 2)
        {
            ViewBag.ErrorKey = "SpreadsheetMerger_Error_MinFiles";
            return View();
        }

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xlsx", ".xls" };
        if (files.Any(f => !allowed.Contains(Path.GetExtension(f.FileName))))
        {
            ViewBag.ErrorKey = "SpreadsheetMerger_Error_InvalidFormat";
            return View();
        }

        try
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.AddWorksheet("Merged");
            var currentRow = 1;
            var headerWritten = false;

            foreach (var file in files)
            {
                using var stream = file.OpenReadStream();
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
                });

                var table = dataset.Tables[0];
                var startRow = headerWritten ? 1 : 0;

                for (var row = startRow; row < table.Rows.Count; row++)
                {
                    for (var col = 0; col < table.Columns.Count; col++)
                    {
                        var value = table.Rows[row][col];
                        var cell = sheet.Cell(currentRow, col + 1);

                        switch (value)
                        {
                            case null:
                            case DBNull:
                                cell.Value = "";
                                break;
                            case double d:
                                cell.Value = d;
                                break;
                            case float f:
                                cell.Value = (double)f;
                                break;
                            case int i:
                                cell.Value = (double)i;
                                break;
                            case long l:
                                cell.Value = (double)l;
                                break;
                            case DateTime dt:
                                cell.Value = dt;
                                break;
                            case bool b:
                                cell.Value = b;
                                break;
                            default:
                                cell.Value = value.ToString() ?? "";
                                break;
                        }
                    }
                    currentRow++;
                }

                headerWritten = true;
            }

            using var output = new MemoryStream();
            workbook.SaveAs(output);
            return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "merged.xlsx");
        }
        catch
        {
            ViewBag.ErrorKey = "SpreadsheetMerger_Error_Processing";
            return View();
        }
    }

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
