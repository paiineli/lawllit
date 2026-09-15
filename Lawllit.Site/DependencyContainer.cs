using Lawllit.Repository;
using Lawllit.Site.Common;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Threading.RateLimiting;

namespace Lawllit.Site;

public static class DependencyContainer
{
    public const string PoliticaLimiteContato = "contato";

    public static IServiceCollection AddLawllitSite(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLawllitRepository(configuration).AddRepositoriosContato();

        services.AddHttpClient();
        services.AddSingleton<ITurnstileValidador, TurnstileValidador>();

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

    private static void AddLimitador(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Por IP, e não balde único, senão três envios de qualquer um fechavam o formulário
            // para o mundo todo. Janela larga porque cada envio custa um e-mail disparado.
            options.AddPolicy(PoliticaLimiteContato, contexto => RateLimitPartition.GetFixedWindowLimiter(
                contexto.Connection.RemoteIpAddress?.ToString() ?? "sem-ip",
                chave => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(10),
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

        services.Configure<BrotliCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Optimal);

        services.Configure<GzipCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Optimal);
    }
}
