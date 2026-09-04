using Lawllit.Repository.Common;
using Lawllit.Repository.Finance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lawllit.Repository;

public static class DependencyContainer
{
    public static IServiceCollection AddLawllitRepository(this IServiceCollection services, IConfiguration configuration)
    {
        var apiUrl = configuration["Api:Url"]
            ?? throw new InvalidOperationException("Api__Url não configurada no ambiente.");

        services.AddHttpContextAccessor();
        services.AddTransient<TokenHttpHandler>();

        services.AddHttpClient(ApiClient.HttpClientName, httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiUrl.TrimEnd('/') + "/");
                httpClient.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<TokenHttpHandler>();

        services.AddScoped<IAuthREP, AuthREP>();
        services.AddScoped<ICategoryREP, CategoryREP>();
        services.AddScoped<IDashboardREP, DashboardREP>();
        services.AddScoped<IProfileREP, ProfileREP>();
        services.AddScoped<IQuoteREP, QuoteREP>();
        services.AddScoped<ITransactionREP, TransactionREP>();
        services.AddScoped<IWelcomeREP, WelcomeREP>();

        return services;
    }
}
