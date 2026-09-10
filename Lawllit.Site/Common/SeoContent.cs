using Lawllit.Model.Common;
using System.Text;
using System.Xml.Linq;

namespace Lawllit.Site.Common;

// robots.txt e sitemap.xml montados em código em vez de arquivo estático, porque as
// URLs precisam sair do host da requisição para sobreviver a uma troca de domínio.
public static class SeoContent
{
    // O que o robô deve indexar. O resto do app financeiro exige login, então fica
    // fora daqui e é bloqueado no robots.
    private static readonly string[] IndexablePaths =
    [
        "/",
        "/Curriculum",
        "/Tools",
        "/Tools/SpreadsheetMerger",
        "/Tools/PdfMerger",
        "/Tools/ImageCompressor",
        "/Tools/Base64",
        "/Tools/TextCase",
        "/Tools/SqlList",
        "/Tools/CharCounter",
        "/Tools/CpfCnpj",
        "/Tools/CepLookup",
        "/Tools/QrCode",
        "/Tools/PasswordGenerator",
        "/Finance",
        "/Finance/Legal/Privacy",
        "/Finance/Legal/Terms",
    ];

    private static readonly string[] DisallowedPaths =
    [
        "/Finance/Auth",
        "/Finance/Dashboard",
        "/Finance/Transaction",
        "/Finance/Category",
        "/Finance/Profile",
        "/Finance/Quotes",
        "/Finance/Welcome",
        "/Finance/Error",
        "/Home/Error",
    ];

    public static string Robots(HttpRequest request)
    {
        var content = new StringBuilder()
            .AppendLine("User-agent: *")
            .AppendLine("Allow: /");

        foreach (var path in DisallowedPaths)
            content.AppendLine($"Disallow: {path}");

        return content
            .AppendLine()
            .AppendLine($"Sitemap: {SeoUrls.Absolute(request, "/sitemap.xml")}")
            .ToString();
    }

    public static string Sitemap(HttpRequest request)
    {
        XNamespace sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xhtml = "http://www.w3.org/1999/xhtml";

        var urls = IndexablePaths.Select(path =>
        {
            var entry = new XElement(sitemap + "url",
                new XElement(sitemap + "loc", SeoUrls.Absolute(request, path)));

            // O conjunto de alternativos precisa aparecer em toda entrada, incluindo a
            // do próprio idioma, senão o Google descarta o grupo inteiro.
            foreach (var language in Constants.ValidLanguages)
                entry.Add(Alternate(xhtml, language, SeoUrls.Absolute(request, path, language)));

            entry.Add(Alternate(xhtml, "x-default", SeoUrls.Absolute(request, path)));

            return entry;
        });

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(sitemap + "urlset",
                new XAttribute(XNamespace.Xmlns + "xhtml", xhtml),
                urls));

        return $"{document.Declaration}{Environment.NewLine}{document}";
    }

    private static XElement Alternate(XNamespace xhtml, string language, string url)
        => new(xhtml + "link",
            new XAttribute("rel", "alternate"),
            new XAttribute("hreflang", language),
            new XAttribute("href", url));
}
