using System.Net;

namespace Lawllit.Site.Common;

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

        // data: é o favicon embutido. blob: é o arquivo que o compressor e o unificador geram.
        "img-src 'self' data: blob:; " +

        // jsdelivr serve pdf-lib, pdf.js, xlsx e qrcodejs. challenges.cloudflare.com é o Turnstile.
        "script-src 'self' https://cdn.jsdelivr.net https://challenges.cloudflare.com; " +
        "frame-src https://challenges.cloudflare.com; " +

        // Worker de outra origem é recusado, então o pdf.js embrulha o do jsdelivr num blob.
        "worker-src blob:; " +

        // A consulta de CEP é a única chamada do navegador para fora.
        "connect-src 'self' https://viacep.com.br";

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
