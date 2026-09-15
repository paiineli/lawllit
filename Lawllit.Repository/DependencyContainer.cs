using Lawllit.Repository.Common;
using Lawllit.Repository.Contato;
using Lawllit.Repository.Finance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lawllit.Repository;

public static class DependencyContainer
{
    // Só a infra compartilhada. Os repositórios ficam separados porque o portfólio não usa os do finance.
    public static IServiceCollection AddLawllitRepository(this IServiceCollection services, IConfiguration configuration)
    {
        // Conferidas na subida, e não na primeira requisição, senão a chave faltando vira 500 no login.
        var urlApi = configuration["Api:Url"]
            ?? throw new InvalidOperationException("Api__Url não configurada no ambiente.");

        if (string.IsNullOrWhiteSpace(configuration["Api:Key"]))
            throw new InvalidOperationException("Api__Key não configurada no ambiente. Deve ter o mesmo valor da API.");

        services.AddHttpContextAccessor();
        services.AddTransient<TokenHttpHandler>();

        services.AddHttpClient(ApiClient.NomeHttpClient, httpClient =>
            {
                httpClient.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
                httpClient.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<TokenHttpHandler>();

        return services;
    }

    public static IServiceCollection AddRepositoriosContato(this IServiceCollection services)
    {
        services.AddScoped<IContatoRepository, ContatoRepository>();

        return services;
    }

    public static IServiceCollection AddRepositoriosFinance(this IServiceCollection services)
    {
        services.AddScoped<IAutenticacaoRepository, AutenticacaoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ICotacaoRepository, CotacaoRepository>();
        services.AddScoped<IPainelRepository, PainelRepository>();
        services.AddScoped<IPerfilRepository, PerfilRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();

        return services;
    }
}
