using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Site.Controllers;

// Toda ferramenta roda no navegador. O controller só entrega a página, nada é processado aqui.
public class FerramentasController : Controller
{
    public IActionResult Index() => View();

    public IActionResult JuntarPlanilhas() => View();

    public IActionResult JuntarPdf() => View();

    public IActionResult ComprimirImagem() => View();

    public IActionResult Base64() => View();

    public IActionResult ListaSql() => View();

    public IActionResult CpfCnpj() => View();

    public IActionResult ConsultaCep() => View();

    public IActionResult QrCode() => View();

    public IActionResult GerarSenha() => View();
}
