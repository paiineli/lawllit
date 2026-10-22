using Lawllit.Repository;
using Lawllit.Site.Common;
using Lawllit.Web;

namespace Lawllit.Site;

public static class DependencyContainer
{
    public const string PoliticaLimiteContato = "contato";

    public static IServiceCollection AddLawllitSite(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLawllitRepository(configuration).AddRepositoriosContato().AddRepositoriosJogos();

        services.AddHttpClient();
        services.AddSingleton<ITurnstileValidador, TurnstileValidador>();

        services.AddLawllitWeb();

        // Janela larga porque cada envio custa um e-mail disparado.
        services.AddLimitadorPorIp(PoliticaLimiteContato, limite: 3, janela: TimeSpan.FromMinutes(10));

        return services;
    }
}
