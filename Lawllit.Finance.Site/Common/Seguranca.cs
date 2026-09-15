using System.Net;

namespace Lawllit.Finance.Site.Common;

public static class Seguranca
{
    // Biblioteca de CDN nova exige origem nova aqui, senão o navegador recusa em silêncio.
    private const string PoliticaConteudo =
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

    public static IApplicationBuilder UseCabecalhosDeSeguranca(this IApplicationBuilder app)
        => app.Use(async (contexto, proximo) =>
        {
            var cabecalhos = contexto.Response.Headers;

            cabecalhos.ContentSecurityPolicy = PoliticaConteudo;
            cabecalhos.XContentTypeOptions = "nosniff";
            cabecalhos.XFrameOptions = "DENY";
            cabecalhos.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            cabecalhos.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), interest-cohort=()");

            await proximo();
        });

    // Sem isto, todo mundo atrás do mesmo nó da Cloudflare cai na mesma partição do limitador.
    // O cabeçalho é falsificável fora da borda, então *.up.railway.app fica desligado na Railway.
    public static IApplicationBuilder UseIpDaCloudflare(this IApplicationBuilder app)
        => app.Use(async (contexto, proximo) =>
        {
            if (IPAddress.TryParse(contexto.Request.Headers["CF-Connecting-IP"], out var ip))
                contexto.Connection.RemoteIpAddress = ip;

            await proximo();
        });
}
