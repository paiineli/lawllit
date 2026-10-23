using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Jogos.Contratos;
using Lawllit.Repository.Jogos;
using Lawllit.Site.Common;
using Lawllit.Site.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Site.Controllers;

public sealed class JogosController(
    IAgendaRepository agendaRepositorio,
    IShrineRepository shrineRepositorio,
    IBaroRepository baroRepositorio,
    IAtualizacaoRepository atualizacaoRepositorio) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Uma busca de patches para os cinco cards, em vez de uma por card.
        var atualizacoes = atualizacaoRepositorio.Listar(cancellationToken);

        return View(await Task.WhenAll(Jogos.Todos.Select(jogo => Montar(jogo, atualizacoes, cancellationToken))));
    }

    public Task<IActionResult> Valorant(CancellationToken cancellationToken) => Pagina(JogoEnum.VALORANT, cancellationToken);

    public Task<IActionResult> Lol(CancellationToken cancellationToken) => Pagina(JogoEnum.LOL, cancellationToken);

    public Task<IActionResult> Dota2(CancellationToken cancellationToken) => Pagina(JogoEnum.DOTA2, cancellationToken);

    public Task<IActionResult> Dbd(CancellationToken cancellationToken) => Pagina(JogoEnum.DBD, cancellationToken);

    public Task<IActionResult> Warframe(CancellationToken cancellationToken) => Pagina(JogoEnum.WARFRAME, cancellationToken);

    private async Task<IActionResult> Pagina(JogoEnum codigo, CancellationToken cancellationToken)
        => View("Jogo", await Montar(Jogos.Por(codigo), atualizacaoRepositorio.Listar(cancellationToken), cancellationToken));

    // Cada jogo busca só a parte que tem. No hub os cinco rodam juntos, e cada fonte tem cache
    // próprio na API, então nenhum espera o outro.
    private async Task<PainelJogoViewModel> Montar(
        Jogo jogo,
        Task<Resultado<List<AtualizacaoModel>>> atualizacoes,
        CancellationToken cancellationToken)
    {
        var agenda = AgendaModel.JogosDisponiveis.Contains(jogo.Codigo) ? agendaRepositorio.Listar(jogo.Codigo, cancellationToken) : null;
        var shrine = jogo.Codigo == JogoEnum.DBD ? shrineRepositorio.Buscar(cancellationToken) : null;
        var baro = jogo.Codigo == JogoEnum.WARFRAME ? baroRepositorio.Buscar(cancellationToken) : null;

        var patches = (await atualizacoes).Valor ?? [];

        return new PainelJogoViewModel
        {
            Jogo = jogo,
            Agora = HorarioBrasil.Agora,
            Agenda = agenda is null ? null : await agenda,
            Shrine = shrine is null ? null : await shrine,
            Baro = baro is null ? null : await baro,

            // O TFT não tem página própria e entra no LoL, que é onde a galera procura.
            Atualizacoes = [.. patches.Where(patch => patch.Jogo == jogo.Codigo
                || (jogo.Codigo == JogoEnum.LOL && patch.Jogo == JogoEnum.TFT))],
        };
    }
}
