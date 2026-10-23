using Lawllit.Finance.Site.Common;
using Lawllit.Repository;
using Lawllit.Web;
using Microsoft.AspNetCore.Authentication.Cookies;

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

        services.AddLawllitWeb();
        services.AddLimitadorPorIp(PoliticaLimiteAcesso, limite: 5, janela: TimeSpan.FromMinutes(1));

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
                // Sem renovação deslizante: o cookie vence quando o token da API vence, nem antes nem depois.
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = false;
                options.Events.OnValidatePrincipal = SessaoFinance.Validar;

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
}
