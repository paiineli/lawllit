using Lawllit.Model.Common;
using System.Globalization;

namespace Lawllit.Site.Common;

// As URLs de SEO saem do host da requisição, e não de um domínio escrito na mão.
// Assim o dia de mover o app para um subdomínio não mexe em nenhuma view.
public static class SeoUrls
{
    // O idioma vem de claim e de cookie, então as duas versões de uma página moram no
    // mesmo endereço, e sem endereço distinto o Google não indexa as duas. Por isso o
    // alternativo carrega ?culture=. O provedor de query string já vem registrado no
    // pipeline de localização, atrás da claim e do cookie, então resolve para o robô,
    // que não tem nem uma nem outro.
    private const string CultureQueryKey = "culture";

    public static string Origin(HttpRequest request)
        => $"{request.Scheme}://{request.Host}{request.PathBase}";

    public static string Absolute(HttpRequest request, string path)
        => Origin(request) + path;

    public static string Absolute(HttpRequest request, string path, string culture)
        => culture == Constants.DefaultLanguage
            ? Absolute(request, path)
            : $"{Absolute(request, path)}?{CultureQueryKey}={culture}";

    // A query da requisição fica fora de propósito. Filtro de tela não é página nova,
    // e entrar no canônico criaria endereço duplicado para o mesmo conteúdo.
    public static string Canonical(HttpRequest request)
        => Absolute(request, request.Path, CultureInfo.CurrentUICulture.Name);

    public static string ForCulture(HttpRequest request, string culture)
        => Absolute(request, request.Path, culture);
}
