using Lawllit.Api.Common;
using Lawllit.Model.Jogos.Contratos;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Lawllit.Api.Jogos.Services;

// Escudo de time de Dota, que é o que o próprio time sobe na Steam. É o único jogo da agenda
// com fonte aberta para isso, e o nome é casado com o que a Liquipedia escreve.
public sealed class OpenDotaService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<OpenDotaService> logger) : IOpenDotaService
{
    private const string ChaveCache = "jogos:dota:escudos";

    // A lista inteira vem numa chamada só, e time troca de escudo raramente.
    private static readonly TimeSpan Validade = TimeSpan.FromDays(1);
    private static readonly TimeSpan ValidadeFalha = TimeSpan.FromMinutes(30);

    // Só os CDNs que a CSP do site libera. Escudo em outro endereço fica de fora em vez de quebrado.
    private static readonly string[] OrigensAceitas =
    [
        "https://cdn.steamusercontent.com/",
        "https://steamcdn-a.akamaihd.net/",
        "https://steamusercontent-a.akamaihd.net/",
    ];

    public async Task PreencherEscudos(AgendaModel agenda, CancellationToken cancellationToken)
    {
        var escudos = await Escudos(cancellationToken);

        foreach (var partida in agenda.Proximas.Concat(agenda.Resultados))
        {
            partida.EquipeA.Logo = escudos.GetValueOrDefault(Normalizar(partida.EquipeA.Nome));
            partida.EquipeB.Logo = escudos.GetValueOrDefault(Normalizar(partida.EquipeB.Nome));
        }
    }

    private async Task<Dictionary<string, string>> Escudos(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(ChaveCache, out Dictionary<string, string>? emCache) && emCache is not null)
            return emCache;

        var url = configuration["Games:OpenDotaTeamsUrl"];
        var times = string.IsNullOrWhiteSpace(url)
            ? null
            : await httpClientFactory.LerJson<List<TimeOpenDotaApiModel>>(url, logger, cancellationToken);

        var escudos = new Dictionary<string, string>();

        // A lista vem do maior rating para o menor, então num nome repetido fica o time mais ativo.
        foreach (var time in times ?? [])
        {
            if (time.Logo is not { } logo || !OrigensAceitas.Any(origem => logo.StartsWith(origem, StringComparison.OrdinalIgnoreCase)))
                continue;

            // A Liquipedia escreve "Aurora" onde a Steam tem "Aurora.1xBet", e às vezes usa só a sigla.
            foreach (var nome in new[] { time.Nome, time.Nome.Split('.')[0], time.Sigla })
            {
                var chave = Normalizar(nome);

                if (chave.Length > 0)
                    escudos.TryAdd(chave, logo);
            }
        }

        cache.Set(ChaveCache, escudos, escudos.Count > 0 ? Validade : ValidadeFalha);

        return escudos;
    }

    // Sem acento, sem pontuação, minúsculo e sem o "team" da frente, que uma fonte usa e a outra não.
    private static string Normalizar(string? nome)
    {
        var texto = new StringBuilder();

        foreach (var letra in (nome ?? string.Empty).Normalize(NormalizationForm.FormD))
        {
            if (char.IsLetterOrDigit(letra) && CharUnicodeInfo.GetUnicodeCategory(letra) != UnicodeCategory.NonSpacingMark)
                texto.Append(char.ToLowerInvariant(letra));
        }

        var normalizado = texto.ToString();

        return normalizado.StartsWith("team", StringComparison.Ordinal) && normalizado.Length > 4 ? normalizado[4..] : normalizado;
    }
}

// Formato cru do OpenDota, só existe dentro da API.
internal sealed class TimeOpenDotaApiModel
{
    [JsonPropertyName("name")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("tag")]
    public string Sigla { get; set; } = string.Empty;

    [JsonPropertyName("logo_url")]
    public string? Logo { get; set; }
}

#region Interfaces

public interface IOpenDotaService
{
    // Sem fonte ou sem correspondência o time só fica sem escudo, e a agenda segue igual.
    Task PreencherEscudos(AgendaModel agenda, CancellationToken cancellationToken);
}

#endregion
