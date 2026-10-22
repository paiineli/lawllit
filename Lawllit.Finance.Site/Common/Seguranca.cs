namespace Lawllit.Finance.Site.Common;

public static class Seguranca
{
    // Biblioteca de CDN nova exige origem nova aqui, senão o navegador recusa em silêncio.
    public const string PoliticaConteudo =
        "default-src 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "frame-ancestors 'none'; " +
        "form-action 'self'; " +
        "style-src 'self'; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +

        // data: é o favicon em SVG embutido no layout.
        "img-src 'self' data:; " +

        // jsdelivr serve o Chart.js dos dois gráficos do painel.
        "script-src 'self' https://cdn.jsdelivr.net";
}
