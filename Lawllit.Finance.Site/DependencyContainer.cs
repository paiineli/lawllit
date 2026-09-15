using Lawllit.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Threading.RateLimiting;

namespace Lawllit.Finance.Site;

public static class DependencyContainer
{
    public const string PoliticaLimiteAcesso = "acesso";

    public static IServiceCollection AddLawllitFinanceSite(
        this IServiceCollection services,
        IConfiguration configuration,
        bool desenvolvimento)
    {
        services.AddLawllitRepository(configuration).AddRepositoriosFinance();

        AddAutenticacao(services, desenvolvimento);
        AddCompressao(services);
        AddLimitador(services);

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

    private static void AddAutenticacao(IServiceCollection services, bool desenvolvimento)
    {
        // Em desenvolvimento o site roda em http, e exigir https travaria o login local.
        var exigenciaHttps = desenvolvimento
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                // Único lugar com rota literal: o middleware soma o PathBase sozinho.
                options.LoginPath = "/Conta/Entrar";
                options.LogoutPath = "/Conta/Sair";
                options.AccessDeniedPath = "/Conta/Entrar";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;

                // Lax e não Strict: o link de confirmação chega de fora, e com Strict a sessão
                // recém-criada não acompanharia a navegação vinda do e-mail.
                options.Cookie.Name = "lawllit.finance";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = exigenciaHttps;
            });

        services.AddAuthorization();

        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "lawllit.finance.antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = exigenciaHttps;
        });
    }

    private static void AddLimitador(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Por IP, e não balde único, senão cinco tentativas de qualquer um travavam o
            // login para o mundo todo.
            options.AddPolicy(PoliticaLimiteAcesso, contexto => RateLimitPartition.GetFixedWindowLimiter(
                contexto.Connection.RemoteIpAddress?.ToString() ?? "sem-ip",
                chave => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }

    private static void AddCompressao(IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            // O UseForwardedHeaders reescreve o esquema para https, então sem isto a compressão
            // ficaria desligada justamente em produção.
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        // Medido no bootstrap.min.css: Fastest entrega arquivo maior que o CDN, e SmallestSize
        // custa quase meio segundo de CPU por requisição.
        services.Configure<BrotliCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Optimal);

        services.Configure<GzipCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Optimal);
    }
}
