using Lawllit.Api.Common;
using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Jogos.Contratos;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;

namespace Lawllit.Api.Jogos.Services;

public sealed class AtualizacaoService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<AtualizacaoService> logger) : IAtualizacaoService
{
    private const string ChaveCache = "jogos:atualizacoes";
    private const string MensagemErro = "Não foi possível carregar as atualizações agora.";

    // Patch sai de duas em duas semanas, então três horas de atraso não fazem falta.
    private static readonly TimeSpan Validade = TimeSpan.FromHours(3);

    // A Riot já trocou o endereço das notas três vezes desde 2025. Quando o montado não
    // existe, o link cai na lista de notas, que não muda.
    private const string NotasLol = "https://www.leagueoflegends.com/pt-br/news/game-updates/league-of-legends-patch-{0}-{1}-notes/";
    private const string ListaNotasLol = "https://www.leagueoflegends.com/pt-br/news/game-updates/";
    private const string NotasValorant = "https://playvalorant.com/pt-br/news/game-updates/valorant-patch-notes-{0}-{1}/";
    private const string ListaNotasValorant = "https://playvalorant.com/pt-br/news/game-updates/";

    // O TFT numera por Set, e nenhuma fonte aberta diz o patch atual. Fica só a lista de notas.
    private const string ListaNotasTft = "https://teamfighttactics.leagueoflegends.com/pt-br/news/game-updates/";

    private static readonly (JogoEnum Jogo, int AppId)[] JogosSteam =
    [
        (JogoEnum.DOTA2, 570),
        (JogoEnum.DBD, 381210),
    ];

    // Nem todo post de patch vem com a tag, então o título serve de segunda checagem.
    private static readonly string[] PalavrasDePatch = ["patch", "update", "hotfix"];

    public Task<Resultado<List<AtualizacaoModel>>> Listar(CancellationToken cancellationToken)
        => cache.BuscarComReserva(ChaveCache, Validade, Consultar, cancellationToken);

    private async Task<Resultado<List<AtualizacaoModel>>> Consultar(CancellationToken cancellationToken)
    {
        // Cada jogo falha sozinho. Os outros aparecem mesmo com uma fonte fora do ar.
        var consultas = new List<Task<AtualizacaoModel?>>
        {
            BuscarLol(cancellationToken),
            BuscarValorant(cancellationToken),
            BuscarWarframe(cancellationToken),
        };

        consultas.AddRange(JogosSteam.Select(jogo => BuscarSteam(jogo.Jogo, jogo.AppId, cancellationToken)));

        var atualizacoes = (await Task.WhenAll(consultas)).OfType<AtualizacaoModel>().ToList();

        if (atualizacoes.Count == 0)
            return Resultado<List<AtualizacaoModel>>.Falha(MensagemErro);

        atualizacoes.Add(new AtualizacaoModel
        {
            Jogo = JogoEnum.TFT,
            Titulo = "notas de patch",
            Link = ListaNotasTft,
        });

        return Resultado<List<AtualizacaoModel>>.Ok([.. atualizacoes.OrderBy(atualizacao => atualizacao.Jogo)]);
    }

    private async Task<AtualizacaoModel?> BuscarLol(CancellationToken cancellationToken)
    {
        var url = Url("Games:LolVersionsUrl");

        if (url is null) return null;

        var versoes = await httpClientFactory.LerJson<string[]>(url, logger, cancellationToken);

        if (!TentarLerVersao(versoes?.FirstOrDefault(), out var principal, out var secundaria)) return null;

        // O Data Dragon chama de 16.19 o patch que o jogo anuncia como 26.19.
        var patch = principal + 10;
        var link = string.Format(NotasLol, patch, secundaria);

        return new AtualizacaoModel
        {
            Jogo = JogoEnum.LOL,
            Titulo = $"patch {patch}.{secundaria}",
            Link = await LinkOuLista(link, ListaNotasLol, cancellationToken),
        };
    }

    private async Task<AtualizacaoModel?> BuscarValorant(CancellationToken cancellationToken)
    {
        var url = Url("Games:ValorantVersionUrl");

        if (url is null) return null;

        var retorno = await httpClientFactory.LerJson<VersaoValorantApiModel>(url, logger, cancellationToken);

        if (!TentarLerVersao(retorno?.Dados?.Versao, out var principal, out var secundaria)) return null;

        // O endereço das notas usa dois dígitos nas duas partes, 13-06 e não 13-6.
        var link = string.Format(NotasValorant, principal.ToString("00"), secundaria.ToString("00"));

        return new AtualizacaoModel
        {
            Jogo = JogoEnum.VALORANT,
            Titulo = $"patch {principal}.{secundaria:00}",
            Link = await LinkOuLista(link, ListaNotasValorant, cancellationToken),
        };
    }

    // A DE não marca o patch nos posts da Steam, mas marca nas notícias do jogo, que o
    // warframestat.us repassa. Só a lista em inglês traz essa marca.
    private async Task<AtualizacaoModel?> BuscarWarframe(CancellationToken cancellationToken)
    {
        var urlBase = Url("Games:WarframeUrl");

        if (urlBase is null) return null;

        var noticias = await httpClientFactory.LerJson<List<NoticiaWarframeApiModel>>(
            $"{urlBase.TrimEnd('/')}/pc/news",
            logger,
            cancellationToken);

        var patch = noticias?.Where(noticia => noticia.Patch).MaxBy(noticia => noticia.Data);

        if (patch is null) return null;

        return new AtualizacaoModel
        {
            Jogo = JogoEnum.WARFRAME,
            Titulo = patch.Titulo,
            Data = HorarioBrasil.DeUtc(patch.Data.UtcDateTime),
            Link = patch.Link,
        };
    }

    private async Task<AtualizacaoModel?> BuscarSteam(JogoEnum jogo, int appId, CancellationToken cancellationToken)
    {
        var url = Url("Games:SteamNewsUrl");

        if (url is null) return null;

        // maxlength corta o corpo do post, que a página não mostra, e deixa a resposta leve.
        var retorno = await httpClientFactory.LerJson<NoticiasSteamApiModel>(
            $"{url}?appid={appId}&count=15&maxlength=1&feeds=steam_community_announcements",
            logger,
            cancellationToken);

        var noticia = retorno?.Conteudo?.Noticias.FirstOrDefault(EhPatch);

        if (noticia is null) return null;

        return new AtualizacaoModel
        {
            Jogo = jogo,
            Titulo = noticia.Titulo,
            Data = HorarioBrasil.DeUnix(noticia.Data),
            Link = noticia.Link,
        };
    }

    private static bool EhPatch(NoticiaSteamApiModel noticia)
        => noticia.Tags?.Contains("patchnotes") == true
            || PalavrasDePatch.Any(palavra => noticia.Titulo.Contains(palavra, StringComparison.OrdinalIgnoreCase));

    private async Task<string> LinkOuLista(string link, string lista, CancellationToken cancellationToken)
        => await httpClientFactory.Existe(link, cancellationToken) ? link : lista;

    private static bool TentarLerVersao(string? versao, out int principal, out int secundaria)
    {
        principal = secundaria = 0;
        var partes = versao?.Split('.');

        return partes is { Length: >= 2 }
            && int.TryParse(partes[0], out principal)
            && int.TryParse(partes[1], out secundaria);
    }

    private string? Url(string chave)
    {
        var url = configuration[chave];

        if (string.IsNullOrWhiteSpace(url))
        {
            logger.LogError("{Chave} não configurada, atualização ignorada.", chave.Replace(":", "__"));
            return null;
        }

        return url;
    }
}

// Formatos crus do valorant-api.com, da Steam e do warframestat.us, só existem dentro da API.
internal sealed class VersaoValorantApiModel
{
    [JsonPropertyName("data")]
    public VersaoValorantDadosApiModel? Dados { get; set; }
}

internal sealed class VersaoValorantDadosApiModel
{
    [JsonPropertyName("version")]
    public string? Versao { get; set; }
}

internal sealed class NoticiasSteamApiModel
{
    [JsonPropertyName("appnews")]
    public NoticiasSteamConteudoApiModel? Conteudo { get; set; }
}

internal sealed class NoticiasSteamConteudoApiModel
{
    [JsonPropertyName("newsitems")]
    public List<NoticiaSteamApiModel> Noticias { get; set; } = [];
}

internal sealed class NoticiaSteamApiModel
{
    [JsonPropertyName("title")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Link { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public long Data { get; set; }

    // Opcional, falta em boa parte dos posts.
    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }
}

internal sealed class NoticiaWarframeApiModel
{
    [JsonPropertyName("message")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTimeOffset Data { get; set; }

    [JsonPropertyName("update")]
    public bool Patch { get; set; }
}

#region Interfaces

public interface IAtualizacaoService
{
    // Uma entrada por jogo, com o patch mais recente. Jogo cuja fonte falhou fica de fora.
    Task<Resultado<List<AtualizacaoModel>>> Listar(CancellationToken cancellationToken);
}

#endregion
