using Lawllit.Api.Common;
using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;

namespace Lawllit.Api.Jogos.Services;

public sealed class ShrineService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<ShrineService> logger) : IShrineService
{
    private const string ChaveCache = "jogos:shrine";
    private const string MensagemErro = "Não foi possível carregar o Shrine agora.";

    // Troca uma vez por semana. Uma hora pega a virada de terça sem atraso grande.
    private static readonly TimeSpan Validade = TimeSpan.FromHours(1);

    public Task<Resultado<ShrineModel>> Buscar(CancellationToken cancellationToken)
        => cache.BuscarComReserva(ChaveCache, Validade, Consultar, cancellationToken);

    private async Task<Resultado<ShrineModel>> Consultar(CancellationToken cancellationToken)
    {
        var urlBase = configuration["Games:DbdUrl"];

        if (string.IsNullOrWhiteSpace(urlBase))
        {
            logger.LogError("Games__DbdUrl não configurada, Shrine indisponível.");
            return Resultado<ShrineModel>.Falha(MensagemErro);
        }

        // Com includeperkinfo a resposta já traz nome e personagem, sem uma segunda chamada.
        var url = $"{urlBase.TrimEnd('/')}/api/shrine?includeperkinfo=1";
        var retorno = await httpClientFactory.LerJson<ShrineApiModel>(url, logger, cancellationToken);

        if (retorno is null || retorno.Perks.Count == 0 || retorno.Fim == 0)
        {
            logger.LogError("A API do Shrine respondeu sem perk ou sem data de troca.");
            return Resultado<ShrineModel>.Falha(MensagemErro);
        }

        var icones = await Icones([.. retorno.Perks.Select(perk => perk.Nome)], cancellationToken);

        return Resultado<ShrineModel>.Ok(new ShrineModel
        {
            Perks = [.. retorno.Perks.Select(perk => new PerkShrineModel
            {
                Nome = perk.Nome,
                Personagem = perk.Personagem,
                Fragmentos = perk.Fragmentos,
                Icone = icones.GetValueOrDefault(perk.Nome),
            })],

            // A rotação acaba às 14:59:59 UTC, e o segundo a mais mostra a hora cheia da virada.
            Fim = HorarioBrasil.DeUnix(retorno.Fim + 1),
        });
    }

    // Os ícones vêm da wiki oficial do DBD, pelo nome da perk e as quatro numa chamada só. Sem a
    // wiki, ou sem página para a perk, ela aparece só com o nome.
    private async Task<Dictionary<string, string>> Icones(List<string> nomes, CancellationToken cancellationToken)
    {
        var icones = new Dictionary<string, string>();
        var urlWiki = configuration["Games:DbdWikiUrl"];

        if (string.IsNullOrWhiteSpace(urlWiki))
        {
            logger.LogError("Games__DbdWikiUrl não configurada, Shrine sem ícones.");
            return icones;
        }

        var url = $"{urlWiki}?action=query&prop=pageimages&piprop=original&redirects=1&format=json&formatversion=2" +
            $"&titles={Uri.EscapeDataString(string.Join('|', nomes))}";
        var consulta = (await httpClientFactory.LerJson<WikiApiModel>(url, logger, cancellationToken))?.Consulta;

        if (consulta is null) return icones;

        // A wiki pode ajustar a grafia ou redirecionar o título, então cada nome segue o caminho
        // até a página final antes de procurar a imagem.
        var destinos = new Dictionary<string, string>();

        foreach (var troca in consulta.Normalizados.Concat(consulta.Redirecionamentos))
            destinos.TryAdd(troca.De, troca.Para);

        var imagens = consulta.Paginas
            .Where(pagina => pagina.Imagem?.Endereco is not null)
            .ToDictionary(pagina => pagina.Titulo, pagina => pagina.Imagem!.Endereco!);

        foreach (var nome in nomes)
        {
            var titulo = nome;

            for (var passo = 0; passo < 3 && destinos.TryGetValue(titulo, out var proximo); passo++)
                titulo = proximo;

            if (imagens.TryGetValue(titulo, out var imagem))
                icones.TryAdd(nome, imagem);
        }

        return icones;
    }
}

// Formato cru do dbd.tricky.lol, só existe dentro da API porque o Site consome o ShrineModel.
internal sealed class ShrineApiModel
{
    [JsonPropertyName("perks")]
    public List<PerkShrineApiModel> Perks { get; set; } = [];

    [JsonPropertyName("end")]
    public long Fim { get; set; }
}

internal sealed class PerkShrineApiModel
{
    [JsonPropertyName("name")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("character")]
    public string Personagem { get; set; } = string.Empty;

    [JsonPropertyName("shards")]
    public int Fragmentos { get; set; }
}

// Formato cru da API MediaWiki da wiki do DBD, com formatversion=2.
internal sealed class WikiApiModel
{
    [JsonPropertyName("query")]
    public WikiConsultaApiModel? Consulta { get; set; }
}

internal sealed class WikiConsultaApiModel
{
    [JsonPropertyName("pages")]
    public List<WikiPaginaApiModel> Paginas { get; set; } = [];

    [JsonPropertyName("normalized")]
    public List<WikiTrocaApiModel> Normalizados { get; set; } = [];

    [JsonPropertyName("redirects")]
    public List<WikiTrocaApiModel> Redirecionamentos { get; set; } = [];
}

internal sealed class WikiPaginaApiModel
{
    [JsonPropertyName("title")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("original")]
    public WikiImagemApiModel? Imagem { get; set; }
}

internal sealed class WikiImagemApiModel
{
    [JsonPropertyName("source")]
    public string? Endereco { get; set; }
}

internal sealed class WikiTrocaApiModel
{
    [JsonPropertyName("from")]
    public string De { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string Para { get; set; } = string.Empty;
}

#region Interfaces

public interface IShrineService
{
    Task<Resultado<ShrineModel>> Buscar(CancellationToken cancellationToken);
}

#endregion
