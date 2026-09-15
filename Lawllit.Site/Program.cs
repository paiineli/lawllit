using System.Globalization;
using Lawllit.Repository;
using Lawllit.Site;
using Lawllit.Site.Common;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;

// Num container a cultura do processo é invariante, e número e data sairiam no formato errado.
var culturaPadrao = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaPadrao;
CultureInfo.DefaultThreadCurrentUICulture = culturaPadrao;

var builder = WebApplication.CreateBuilder(args);

// Bind em [::] atende IPv4 e IPv6, o que a rede privada da Railway exige.
var porta = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://[::]:{porta}");

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

if (builder.Environment.IsDevelopment())
    TurnstileChaves.AplicarPadraoDesenvolvimento(builder.Configuration);

builder.Services.AddLawllitSite(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

app.UseIpDaCloudflare();

if (!app.Environment.IsDevelopment())
{
    // Falha na subida em vez de deixar o formulário de contato aberto sem verificação.
    foreach (var chave in TurnstileChaves.Obrigatorias)
    {
        if (string.IsNullOrWhiteSpace(app.Configuration[chave]))
            throw new InvalidOperationException($"{chave.Replace(":", "__")} não configurada no ambiente.");
    }

    app.UseExceptionHandler(errorApp => errorApp.Run(context =>
    {
        context.Response.Redirect(context.Request.PathBase + "/Inicio/Erro");
        return Task.CompletedTask;
    }));

    app.UseHsts();
}

app.UseCabecalhosDeSeguranca();

app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        // Com hash na URL o navegador pode guardar por um ano sem servir versão velha. O resto
        // fica em uma hora, para troca de imagem ou de currículo aparecer rápido.
        var temHashNaUrl = context.Context.Request.Query.ContainsKey("v")
            || context.Context.Request.Path.StartsWithSegments("/fonts");

        context.Context.Response.Headers.CacheControl = temHashNaUrl
            ? "public,max-age=31536000,immutable"
            : "public,max-age=3600";
    }
});

app.UseRouting();

// Depois do UseRouting: o [EnableRateLimiting] é metadado do endpoint, e antes da rota
// resolvida o limitador não enxerga política nenhuma e deixa tudo passar.
app.UseRateLimiter();

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" }));

app.MapGet("/robots.txt", (HttpRequest request) =>
    TypedResults.Text(SeoConteudo.Robots(request), "text/plain"));

app.MapGet("/sitemap.xml", (HttpRequest request) =>
    TypedResults.Text(SeoConteudo.Sitemap(request), "application/xml"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Index}/{id?}");

app.Run();
