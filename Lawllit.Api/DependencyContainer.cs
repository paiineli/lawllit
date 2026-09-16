using Lawllit.Api.Common;
using Lawllit.Api.Contato.Repositories;
using Lawllit.Api.Contato.Services;
using Lawllit.Api.Finance.Repositories;
using Lawllit.Api.Finance.Services;

namespace Lawllit.Api;

public static class DependencyContainer
{
    public static IServiceCollection AddLawllitApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBanco(configuration);

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IContatoRepository, ContatoRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddSingleton<IEnviadorEmail, EnviadorEmail>();

        services.AddScoped<GeradorToken>();
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IContatoService, ContatoService>();
        services.AddScoped<ICotacaoService, CotacaoService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPainelService, PainelService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<ITransacaoService, TransacaoService>();

        return services;
    }
}
