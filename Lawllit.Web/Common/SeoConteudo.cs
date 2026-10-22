using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

namespace Lawllit.Web.Common;

// robots.txt e sitemap.xml montados em código em vez de arquivo estático, porque as URLs
// precisam sair do host da requisição para sobreviver a uma troca de domínio.
public static class SeoConteudo
{
    public static string Robots(HttpRequest request, string[] caminhosBloqueados)
    {
        var conteudo = new StringBuilder()
            .AppendLine("User-agent: *")
            .AppendLine("Allow: /");

        foreach (var caminho in caminhosBloqueados)
            conteudo.AppendLine($"Disallow: {caminho}");

        return conteudo
            .AppendLine()
            .AppendLine($"Sitemap: {SeoUrls.Absoluta(request, "/sitemap.xml")}")
            .ToString();
    }

    public static string Sitemap(HttpRequest request, string[] caminhosIndexaveis)
    {
        XNamespace sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var urls = caminhosIndexaveis.Select(caminho => new XElement(sitemap + "url",
            new XElement(sitemap + "loc", SeoUrls.Absoluta(request, caminho))));

        var documento = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(sitemap + "urlset", urls));

        return $"{documento.Declaration}{Environment.NewLine}{documento}";
    }
}
