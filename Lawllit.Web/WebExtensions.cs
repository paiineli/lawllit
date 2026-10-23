using System.Globalization;
using System.IO.Compression;
using System.Threading.RateLimiting;
using Lawllit.Web.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lawllit.Web;

// O que os dois sites fazem igual. Cada um informa só o que é dele: CSP, rota de erro e sitemap.
public static class WebExtensions
{
    public static WebApplicationBuilder UseHospedagemLawllit(this WebApplicationBuilder builder)
    {
        // Num container a cultura do processo é invariante, e número e data sairiam no formato errado.
        var culturaPadrao = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culturaPadrao;
        CultureInfo.DefaultThreadCurrentUICulture = culturaPadrao;

        // Bind em [::] atende IPv4 e IPv6, o que a rede privada da Railway exige.
        var porta = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        builder.WebHost.UseUrls($"http://[::]:{porta}");

        return builder;
    }

    // Sem chave fixa, cada deploy gerava chave nova: todo mundo era deslogado e formulário aberto falhava no antiforgery.
    public static WebApplicationBuilder AddChavesDeProtecao(this WebApplicationBuilder builder, string nomeAplicacao)
    {
        var protecao = builder.Services.AddDataProtection().SetApplicationName(nomeAplicacao);

        if (builder.Environment.IsDevelopment())
            return builder;

        var chave = builder.Configuration["DataProtection:Chave"]
            ?? throw new InvalidOperationException("DataProtection__Chave não configurada no ambiente.");

        protecao.DisableAutomaticKeyGeneration();
        builder.Services.Configure<KeyManagementOptions>(options => options.XmlRepository = new ChaveFixaRepository(chave));

        return builder;
    }

    public static IServiceCollection AddLawllitWeb(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            // O UseForwardedHeaders reescreve o esquema para https, então sem isto a compressão
            // ficaria desligada justamente em produção.
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        // Medido no bootstrap.min.css que o finance servia: Fastest entrega arquivo maior que o CDN, e SmallestSize
        // custa quase meio segundo de CPU por requisição.
        services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
        services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);

        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    // Por IP, e não balde único, senão o limite de um visitante fechava a porta para o mundo todo.
    public static IServiceCollection AddLimitadorPorIp(this IServiceCollection services, string politica, int limite, TimeSpan janela)
        => services.AddRateLimiter(options =>
        {
            options.AddPolicy(politica, contexto => RateLimitPartition.GetFixedWindowLimiter(
                contexto.Connection.RemoteIpAddress?.ToString() ?? "sem-ip",
                chave => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limite,
                    Window = janela,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

    public static WebApplication UseLawllitWeb(this WebApplication app, string politicaConteudo, string rotaErro)
    {
        app.UseForwardedHeaders();
        app.UseIpDaCloudflare();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(errorApp => errorApp.Run(context =>
            {
                context.Response.Redirect(context.Request.PathBase + rotaErro);
                return Task.CompletedTask;
            }));

            app.UseHsts();
        }

        app.UseCabecalhosDeSeguranca(politicaConteudo);
        app.UseResponseCompression();

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context =>
            {
                // Com hash na URL o navegador pode guardar por um ano sem servir versão velha. O resto
                // fica em uma hora, para troca de imagem ou de currículo aparecer rápido.
                var temHashNaUrl = context.Context.Request.Query.ContainsKey("v")
                    || context.Context.Request.Path.StartsWithSegments("/_content/Lawllit.Web/fonts");

                context.Context.Response.Headers.CacheControl = temHashNaUrl
                    ? "public,max-age=31536000,immutable"
                    : "public,max-age=3600";
            }
        });

        return app;
    }

    public static WebApplication MapSeoLawllit(this WebApplication app, string[] caminhosIndexaveis, string[] caminhosBloqueados)
    {
        app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" }));

        app.MapGet("/robots.txt", (HttpRequest request) =>
            TypedResults.Text(SeoConteudo.Robots(request, caminhosBloqueados), "text/plain"));

        app.MapGet("/sitemap.xml", (HttpRequest request) =>
            TypedResults.Text(SeoConteudo.Sitemap(request, caminhosIndexaveis), "application/xml"));

        return app;
    }
}
