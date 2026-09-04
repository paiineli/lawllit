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

app.UseStaticFiles();

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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
