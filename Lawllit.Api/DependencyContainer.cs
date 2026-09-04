using Lawllit.Api.Common;
using Lawllit.Api.Finance.Repositories;
using Lawllit.Api.Finance.Services;

namespace Lawllit.Api;

public static class DependencyContainer
{
    public static IServiceCollection AddLawllitApi(this IServiceCollection services)
    {
        services.AddConnectionFactory();

        services.AddScoped<ICategoryREP, CategoryREP>();
        services.AddScoped<ITransactionREP, TransactionREP>();
        services.AddScoped<IUserREP, UserREP>();

        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IQuotesService, QuotesService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IWelcomeService, WelcomeService>();

        return services;
    }
}
