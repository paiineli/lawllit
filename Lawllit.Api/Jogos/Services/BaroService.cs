using Lawllit.Api.Common;
using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;

namespace Lawllit.Api.Jogos.Services;

public sealed class BaroService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<BaroService> logger) : IBaroService
{
    private const string ChaveCache = "jogos:baro";
    private const string MensagemErro = "Não foi possível carregar o Baro agora.";

    // O jsDelivr espelha o repositório da WFCD, e é o mesmo CDN que o site já usa para scripts.
    private const string ImagemCatalogo = "https://cdn.jsdelivr.net/gh/wfcd/warframe-items@master/data/img/{0}";

    private static readonly TimeSpan Validade = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ValidadeImagem = TimeSpan.FromDays(1);

    public Task<Resultado<BaroModel>> Buscar(CancellationToken cancellationToken)
        => cache.BuscarComReserva(ChaveCache, Validade, Consultar, cancellationToken);

    private async Task<Resultado<BaroModel>> Consultar(CancellationToken cancellationToken)
    {
        var urlBase = configuration["Games:WarframeUrl"];

        if (string.IsNullOrWhiteSpace(urlBase))
        {
            logger.LogError("Games__WarframeUrl não configurada, Baro indisponível.");
            return Resultado<BaroModel>.Falha(MensagemErro);
        }

        var raiz = urlBase.TrimEnd('/');

        // O idioma vai no caminho. Por parâmetro ou cabeçalho a API ignora e responde em inglês.
        var url = $"{raiz}/pt/voidTrader";
        var retorno = await httpClientFactory.LerJson<BaroApiModel>(url, logger, cancellationToken);

        if (retorno is null || retorno.Chegada == default || retorno.Partida == default)
        {
            logger.LogError("A API do Warframe respondeu o Baro sem data de chegada ou de partida.");
            return Resultado<BaroModel>.Falha(MensagemErro);
        }

        var itens = await Task.WhenAll(retorno.Itens.Select(async item => new ItemBaroModel
        {
            Nome = item.Nome,
            Ducats = item.Ducats,
            Creditos = item.Creditos,
            Imagem = await Imagem(raiz, item.CodigoInterno, cancellationToken),
        }));

        return Resultado<BaroModel>.Ok(new BaroModel
        {
            Chegada = HorarioBrasil.DeUtc(retorno.Chegada.UtcDateTime),
            Partida = HorarioBrasil.DeUtc(retorno.Partida.UtcDateTime),
            Local = retorno.Local,
            Itens = [.. itens],
        });
    }

    // A imagem sai do catálogo aberto da WFCD, um item por busca. O estoque só muda de duas em
    // duas semanas, então cada resposta, achada ou não, fica um dia em cache.
    private async Task<string?> Imagem(string raiz, string codigoInterno, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(codigoInterno)) return null;

        var chave = $"jogos:baro:imagem:{codigoInterno}";

        if (cache.TryGetValue(chave, out string? emCache)) return emCache;

        // O estoque usa o caminho da loja, e o catálogo o do item, que é o mesmo sem o /StoreItems.
        var codigoCatalogo = codigoInterno.Replace("/StoreItems", string.Empty);
        var url = $"{raiz}/items/search/{Uri.EscapeDataString(codigoCatalogo)}?by=uniqueName&only=uniqueName,imageName";
        var encontrados = await httpClientFactory.LerJson<List<ItemCatalogoApiModel>>(url, logger, cancellationToken);

        // Só vale a correspondência exata. A busca também devolve parecidos, que seriam outra imagem.
        var arquivo = encontrados?.FirstOrDefault(item => item.CodigoInterno == codigoCatalogo)?.Arquivo;
        var imagem = string.IsNullOrWhiteSpace(arquivo) ? null : string.Format(ImagemCatalogo, arquivo);

        cache.Set(chave, imagem, ValidadeImagem);

        return imagem;
    }
}

// Formato cru do warframestat.us, só existe dentro da API porque o Site consome o BaroModel.
internal sealed class BaroApiModel
{
    [JsonPropertyName("activation")]
    public DateTimeOffset Chegada { get; set; }

    [JsonPropertyName("expiry")]
    public DateTimeOffset Partida { get; set; }

    [JsonPropertyName("location")]
    public string Local { get; set; } = string.Empty;

    [JsonPropertyName("inventory")]
    public List<ItemBaroApiModel> Itens { get; set; } = [];
}

internal sealed class ItemBaroApiModel
{
    [JsonPropertyName("item")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("uniqueName")]
    public string CodigoInterno { get; set; } = string.Empty;

    // Preço que falta chega como null, então os dois são anuláveis.
    [JsonPropertyName("ducats")]
    public int? Ducats { get; set; }

    [JsonPropertyName("credits")]
    public int? Creditos { get; set; }
}

internal sealed class ItemCatalogoApiModel
{
    [JsonPropertyName("uniqueName")]
    public string CodigoInterno { get; set; } = string.Empty;

    [JsonPropertyName("imageName")]
    public string? Arquivo { get; set; }
}

#region Interfaces

public interface IBaroService
{
    Task<Resultado<BaroModel>> Buscar(CancellationToken cancellationToken);
}

#endregion
