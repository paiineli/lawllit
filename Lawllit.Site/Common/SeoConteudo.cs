using System.Text;
using System.Xml.Linq;

namespace Lawllit.Site.Common;

// robots.txt e sitemap.xml montados em código em vez de arquivo estático, porque as URLs
// precisam sair do host da requisição para sobreviver a uma troca de domínio.
public static class SeoConteudo
{
    private static readonly string[] CaminhosIndexaveis =
    [
        "/",
        "/Ferramentas",
        .. Ferramentas.Todas.Select(ferramenta => $"/Ferramentas/{ferramenta.Acao}"),
    ];

    private static readonly string[] CaminhosBloqueados =
    [
        "/Inicio/Erro",

        // Fragmento HTML da assinatura de e-mail, para copiar e colar. Não é página de busca.
        "/signature.html",
    ];

    public static string Robots(HttpRequest request)
    {
        var conteudo = new StringBuilder()
            .AppendLine("User-agent: *")
            .AppendLine("Allow: /");

        foreach (var caminho in CaminhosBloqueados)
            conteudo.AppendLine($"Disallow: {caminho}");

        return conteudo
            .AppendLine()
            .AppendLine($"Sitemap: {SeoUrls.Absoluta(request, "/sitemap.xml")}")
            .ToString();
    }

    public static string Sitemap(HttpRequest request)
    {
        XNamespace sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var urls = CaminhosIndexaveis.Select(caminho => new XElement(sitemap + "url",
            new XElement(sitemap + "loc", SeoUrls.Absoluta(request, caminho))));

        var documento = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(sitemap + "urlset", urls));

        return $"{documento.Declaration}{Environment.NewLine}{documento}";
    }
}
