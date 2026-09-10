using Lawllit.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Threading.RateLimiting;

namespace Lawllit.Site;

public static class DependencyContainer
{
    private const string ExternalScheme = "External";
    private const string AuthRateLimiterPolicy = "auth";

    public static IServiceCollection AddLawllitSite(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLawllitRepository(configuration);

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                // Esses três caminhos são resolvidos pelo middleware, que já soma o
                // PathBase sozinho, então aqui é o único lugar onde a rota fica literal.
                options.LoginPath = "/Finance/Auth/Login";
                options.LogoutPath = "/Finance/Auth/Logout";
                options.AccessDeniedPath = "/Finance/Auth/Login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            })
            .AddCookie(ExternalScheme, options => options.ExpireTimeSpan = TimeSpan.FromMinutes(5));

        AddGoogleAuthentication(services, configuration);

        services.AddAuthorization();

        AddCompression(services);

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(AuthRateLimiterPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    private static void AddCompression(IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            // A Railway encerra o TLS na borda e o Kestrel recebe HTTP, mas o
            // UseForwardedHeaders reescreve o esquema para https antes daqui, então sem
            // isto a compressão ficaria desligada justamente em produção.
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        // Nível rápido, e não o máximo. Em HTML e CSS a diferença de tamanho entre os
        // dois é de poucos por cento, e o custo de CPU por requisição é bem maior.
        services.Configure<BrotliCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Fastest);

        services.Configure<GzipCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Fastest);
    }

    // Sem credencial configurada o app sobe sem o botão do Google, o que mantém o
    // ambiente local funcionando sem precisar de projeto no Google Cloud.
    private static void AddGoogleAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        var clientSecret = configuration["Authentication:Google:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret)) return;

        services.AddAuthentication().AddGoogle(options =>
        {
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.SignInScheme = ExternalScheme;
        });
    }
}
