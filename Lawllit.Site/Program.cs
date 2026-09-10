using Lawllit.Model.Common;
using Lawllit.Repository;
using Lawllit.Site;
using Lawllit.Site.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Bind em [::] atende IPv4 e IPv6, o que a rede privada da Railway exige.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://[::]:{port}");

builder.Services.AddLocalization();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)));

builder.Services.AddLawllitSite(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp => errorApp.Run(context =>
    {
        // Erro dentro do app financeiro cai na página de erro dele, o resto na do site.
        var originalPath = context.Features.Get<IExceptionHandlerPathFeature>()?.Path ?? string.Empty;

        var errorPath = originalPath.StartsWith("/Finance", StringComparison.OrdinalIgnoreCase)
            ? "/Finance/Error"
            : "/Home/Error";

        context.Response.Redirect(context.Request.PathBase + errorPath);
        return Task.CompletedTask;
    }));

    app.UseHsts();
}

app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        // O asp-append-version manda o hash do arquivo na query, e fonte nova entra com
        // nome novo, então nesses dois casos o navegador pode guardar por um ano sem
        // risco de servir versão velha. O resto fica em uma hora, para troca de imagem
        // ou de currículo aparecer rápido.
        var isFingerprinted = context.Context.Request.Query.ContainsKey("v")
            || context.Context.Request.Path.StartsWithSegments("/fonts");

        context.Context.Response.Headers.CacheControl = isFingerprinted
            ? "public,max-age=31536000,immutable"
            : "public,max-age=3600";
    }
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseRateLimiter();
app.UseRouting();
app.UseAuthentication();

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(Constants.DefaultLanguage)
    .AddSupportedCultures(Constants.ValidLanguages)
    .AddSupportedUICultures(Constants.ValidLanguages);

localizationOptions.RequestCultureProviders.Insert(0, new ClaimCultureProvider());
localizationOptions.RequestCultureProviders.Insert(1, new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);
app.UseAuthorization();

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" })).AllowAnonymous();

app.MapGet("/robots.txt", (HttpRequest request) =>
    TypedResults.Text(SeoContent.Robots(request), "text/plain")).AllowAnonymous();

app.MapGet("/sitemap.xml", (HttpRequest request) =>
    TypedResults.Text(SeoContent.Sitemap(request), "application/xml")).AllowAnonymous();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
