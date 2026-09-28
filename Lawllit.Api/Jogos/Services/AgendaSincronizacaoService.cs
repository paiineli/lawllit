using Lawllit.Model.Common.Enums;
using Lawllit.Model.Jogos.Contratos;

namespace Lawllit.Api.Jogos.Services;

// Lê a agenda em segundo plano porque a Liquipedia aceita um parse a cada 30s, somando todas
// as wikis. Buscar na hora da visita estouraria o limite com duas visitas ao mesmo tempo.
public sealed class AgendaSincronizacaoService(
    ILiquipediaService liquipediaService,
    IOpenDotaService openDotaService,
    IAgendaService agendaService,
    ILogger<AgendaSincronizacaoService> logger) : BackgroundService
{
    private static readonly TimeSpan EsperaEntreLeituras = TimeSpan.FromSeconds(35);
    private static readonly TimeSpan IntervaloCiclo = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var relogio = new PeriodicTimer(IntervaloCiclo);

        do
        {
            foreach (var jogo in AgendaModel.JogosDisponiveis)
            {
                await Sincronizar(jogo, stoppingToken);
                await Task.Delay(EsperaEntreLeituras, stoppingToken);
            }
        }
        while (await relogio.WaitForNextTickAsync(stoppingToken));
    }

    private async Task Sincronizar(JogoEnum jogo, CancellationToken stoppingToken)
    {
        try
        {
            var resultado = await liquipediaService.Buscar(jogo, stoppingToken);

            // Falha não apaga nada, a agenda anterior continua valendo até a próxima volta.
            if (!resultado.Sucesso) return;

            if (jogo == JogoEnum.DOTA2)
                await openDotaService.PreencherEscudos(resultado.Valor!, stoppingToken);

            agendaService.Guardar(resultado.Valor!);
        }
        // Exceção que escapa de um BackgroundService derruba a API inteira, finance junto. Aqui
        // ela vira log, e a próxima volta tenta de novo.
        catch (Exception excecao) when (excecao is not OperationCanceledException)
        {
            logger.LogError(excecao, "Erro inesperado ao sincronizar a agenda de {Jogo}.", jogo);
        }
    }
}
