using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Lawllit.Api.Common;
using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Jogos.Contratos;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Lawllit.Api.Jogos.Services;

public sealed partial class LiquipediaService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<LiquipediaService> logger) : ILiquipediaService
{
    private const string MensagemErro = "Não foi possível carregar a agenda agora.";

    // Próximas e resultados numa chamada só, porque cada parse gasta 30s do limite da
    // Liquipedia. O tier 1 e 2 deixa de fora as séries amadoras, que lotavam a lista.
    private const string Wikitexto =
        "<div id=\"lawllit-proximas\">{{#invoke:Lua|invoke|module=MatchTicker/Custom|fn=mainPage|dev=false|type=upcoming|limit=15|filterbuttons-liquipediatier=1,2}}</div>" +
        "<div id=\"lawllit-resultados\">{{#invoke:Lua|invoke|module=MatchTicker/Custom|fn=mainPage|dev=false|type=recent|limit=10|filterbuttons-liquipediatier=1,2}}</div>";

    // Os mesmos de AgendaModel.JogosDisponiveis, que é a única lista que o sincronizador percorre.
    private static readonly Dictionary<JogoEnum, string> Wikis = new()
    {
        [JogoEnum.VALORANT] = "valorant",
        [JogoEnum.LOL] = "leagueoflegends",
        [JogoEnum.DOTA2] = "dota2",
    };

    public async Task<Resultado<AgendaModel>> Buscar(JogoEnum jogo, CancellationToken cancellationToken)
    {
        var urlBase = configuration["Games:LiquipediaUrl"];

        if (string.IsNullOrWhiteSpace(urlBase))
        {
            logger.LogError("Games__LiquipediaUrl não configurada, agenda indisponível.");
            return Resultado<AgendaModel>.Falha(MensagemErro);
        }

        var raiz = new Uri(urlBase.TrimEnd('/') + "/");
        var url = $"{raiz.AbsoluteUri}{Wikis[jogo]}/api.php?action=parse&format=json&formatversion=2&prop=text" +
            $"&title=Liquipedia:Matches&text={Uri.EscapeDataString(Wikitexto)}";

        var retorno = await httpClientFactory.LerJson<ParseApiModel>(url, logger, cancellationToken);
        var html = retorno?.Parse?.Html;

        if (string.IsNullOrWhiteSpace(html))
        {
            logger.LogError("A Liquipedia respondeu sem HTML para {Jogo}: {Erro}", jogo, retorno?.Erro?.Info);
            return Resultado<AgendaModel>.Falha(MensagemErro);
        }

        var documento = new HtmlParser().ParseDocument(html);
        var proximas = LerPartidas(documento, "#lawllit-proximas", raiz, concluidas: false);
        var resultados = LerPartidas(documento, "#lawllit-resultados", raiz, concluidas: true);

        // Tudo ou nada. Cartão fora do formato quer dizer que o HTML mudou, e aí vale mais manter
        // a agenda anterior do que mostrar uma pela metade. Wiki grande sempre tem resultado.
        if (proximas is null || resultados is null || resultados.Count == 0)
        {
            logger.LogError("O HTML da agenda de {Jogo} veio fora do formato esperado.", jogo);
            return Resultado<AgendaModel>.Falha(MensagemErro);
        }

        return Resultado<AgendaModel>.Ok(new AgendaModel
        {
            Jogo = jogo,
            Proximas = proximas,
            Resultados = resultados,
            AtualizadoEm = HorarioBrasil.Agora,
        });
    }

    private static List<PartidaModel>? LerPartidas(IDocument documento, string seletorSecao, Uri raiz, bool concluidas)
    {
        var secao = documento.QuerySelector(seletorSecao);

        if (secao is null) return null;

        var partidas = new List<PartidaModel>();

        foreach (var cartao in secao.QuerySelectorAll(".match-info"))
        {
            var partida = LerPartida(cartao, raiz, concluidas);

            if (partida is null) return null;

            partidas.Add(partida);
        }

        return partidas;
    }

    private static PartidaModel? LerPartida(IElement cartao, Uri raiz, bool concluida)
    {
        var equipes = cartao.QuerySelectorAll(".match-info-header-opponent");
        var inicio = cartao.QuerySelector(".timer-object[data-timestamp]")?.GetAttribute("data-timestamp");

        if (equipes.Length != 2 || !long.TryParse(inicio, out var segundosUnix)) return null;

        // Só existe placar depois que a partida começa. Antes disso o lugar dele diz "vs".
        var placares = cartao.QuerySelectorAll(".match-info-header-scoreholder-score");
        var temPlacar = placares.Length == 2;

        var formato = Texto(cartao.QuerySelector(".match-info-header-scoreholder-lower")).Trim('(', ')');

        return new PartidaModel
        {
            Campeonato = Texto(cartao.QuerySelector(".match-info-tournament-name") ?? cartao.QuerySelector(".match-info-tournament")),
            EquipeA = LerEquipe(equipes[0], temPlacar ? placares[0] : null),
            EquipeB = LerEquipe(equipes[1], temPlacar ? placares[1] : null),
            Inicio = HorarioBrasil.DeUnix(segundosUnix),
            Formato = formato.Length > 0 ? formato : null,
            Concluida = concluida,
            LinkPartida = LinkAbsoluto(cartao.QuerySelector(".match-info-links a[href*='Match:ID_']"), raiz),
            LinkTransmissao = LinkAbsoluto(cartao.QuerySelector(".match-info-links a[href*='/Special:Stream/']"), raiz),
        };
    }

    private static EquipePartidaModel LerEquipe(IElement equipe, IElement? placar)
    {
        // Time ainda não definido vem como texto solto, tipo "TBD" ou a vaga da chave.
        var nome = Texto(equipe.QuerySelector(".name"));

        if (nome.Length == 0)
            nome = Texto(equipe.QuerySelector(".brkts-opponent-block-literal"));

        return new EquipePartidaModel
        {
            Nome = nome.Length > 0 ? nome : "a definir",
            // Partida que acabou de começar vem com o espaço do placar vazio, e isso não é zero.
            Placar = Texto(placar) is { Length: > 0 } pontos ? pontos : null,
            Vencedora = equipe.ClassList.Contains("match-info-header-winner"),
        };
    }

    private static string Texto(IElement? elemento)
        => elemento is null ? string.Empty : EspacosRepetidos().Replace(elemento.TextContent, " ").Trim();

    private static string? LinkAbsoluto(IElement? link, Uri raiz)
    {
        var endereco = link?.GetAttribute("href");

        return string.IsNullOrWhiteSpace(endereco) ? null : new Uri(raiz, endereco).AbsoluteUri;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosRepetidos();
}

// Formato cru do action=parse, só existe dentro da API porque o Site consome o AgendaModel.
internal sealed class ParseApiModel
{
    [JsonPropertyName("parse")]
    public ParseConteudoApiModel? Parse { get; set; }

    [JsonPropertyName("error")]
    public ParseErroApiModel? Erro { get; set; }
}

internal sealed class ParseConteudoApiModel
{
    [JsonPropertyName("text")]
    public string? Html { get; set; }
}

internal sealed class ParseErroApiModel
{
    [JsonPropertyName("info")]
    public string? Info { get; set; }
}

#region Interfaces

public interface ILiquipediaService
{
    // Uma chamada de parse por vez. Quem chama respeita o intervalo de 30s entre elas.
    Task<Resultado<AgendaModel>> Buscar(JogoEnum jogo, CancellationToken cancellationToken);
}

#endregion
