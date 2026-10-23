using Lawllit.Api.Common;
using Lawllit.Api.Contato.Repositories;
using Lawllit.Api.Contato.Services;
using Lawllit.Api.Finance.Repositories;
using Lawllit.Api.Finance.Services;
using Lawllit.Api.Jogos.Services;

namespace Lawllit.Api;

public static class DependencyContainer
{
    public static IServiceCollection AddLawllitApi(this IServiceCollection services, IConfiguration configuration, bool desenvolvimento)
    {
        services.AddBanco(configuration);
        services.AddClienteJogos();

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IContatoRepository, ContatoRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        AddEnvioEmail(services, configuration, desenvolvimento);

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

    // Lidas na subida e não na hora do envio, senão o cadastro em produção caía no meio, com a conta já gravada.
    private static void AddEnvioEmail(IServiceCollection services, IConfiguration configuration, bool desenvolvimento)
    {
        var remetente = configuration["Email:From"];
        var chaveApi = configuration["Email:ApiKey"];

        if (string.IsNullOrWhiteSpace(remetente) || string.IsNullOrWhiteSpace(chaveApi))
        {
            if (!desenvolvimento)
                throw new InvalidOperationException("Email__From e Email__ApiKey precisam estar configuradas no ambiente.");

            services.AddSingleton<IEnviadorEmail, EnviadorEmailLocal>();
            return;
        }

        services.AddSingleton(new ConfiguracaoEmail(remetente, chaveApi));
        services.AddSingleton<IEnviadorEmail, EnviadorEmail>();
    }
}
