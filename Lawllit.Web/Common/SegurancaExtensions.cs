using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Lawllit.Web.Common;

public static class SegurancaExtensions
{
    public static IApplicationBuilder UseCabecalhosDeSeguranca(this IApplicationBuilder app, string politicaConteudo)
        => app.Use(async (contexto, proximo) =>
        {
            var cabecalhos = contexto.Response.Headers;

            cabecalhos.ContentSecurityPolicy = politicaConteudo;
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
