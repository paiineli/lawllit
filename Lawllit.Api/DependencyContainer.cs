using Lawllit.Api.Common;
using Lawllit.Api.Contato.Repositories;
using Lawllit.Api.Contato.Services;
using Lawllit.Api.Finance.Repositories;
using Lawllit.Api.Finance.Services;
using Lawllit.Api.Jogos.Services;

namespace Lawllit.Api;

public static class DependencyContainer
{
    public static IServiceCollection AddLawllitApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBanco(configuration);
        services.AddClienteJogos();

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

        services.AddScoped<IAtualizacaoService, AtualizacaoService>();
        services.AddScoped<IBaroService, BaroService>();
        services.AddScoped<IShrineService, ShrineService>();

        // Singleton porque o sincronizador vive o processo inteiro e guarda a agenda em memória.
        services.AddSingleton<IAgendaService, AgendaService>();
        services.AddSingleton<ILiquipediaService, LiquipediaService>();
        services.AddSingleton<IOpenDotaService, OpenDotaService>();
        services.AddHostedService<AgendaSincronizacaoService>();

        return services;
    }
}
