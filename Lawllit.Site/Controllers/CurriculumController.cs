using Lawllit.Model.Common;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Lawllit.Site.Controllers;

public class CurriculumController : Controller
{
    // O parâmetro se chama culture porque é o mesmo nome que o provedor de query string
    // da localização já usa, então o hreflang e o sitemap continuam valendo para esta
    // página. A diferença é que aqui ele ganha da claim do usuário logado, senão quem
    // está no finance em português abriria o resume em inglês no idioma errado.
    public IActionResult Index(string? culture)
    {
        if (Constants.ValidLanguages.Contains(culture))
        {
            var chosen = new CultureInfo(culture!);
            CultureInfo.CurrentCulture = chosen;
            CultureInfo.CurrentUICulture = chosen;
        }

        return View();
    }
}
