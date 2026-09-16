using Lawllit.Model.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lawllit.Repository.Common;

public static class ApiClient
{
    public const string NomeHttpClient = "LawllitApi";

    // Mesmo contrato de serialização das duas pontas, camelCase com enum em texto.
    public static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<Resultado> Enviar(
        this HttpClient httpClient,
        HttpMethod metodo,
        string url,
        object? corpo,
        CancellationToken cancellationToken)
    {
        using var resposta = await Requisitar(httpClient, metodo, url, corpo, cancellationToken);

        if (resposta.IsSuccessStatusCode)
            return Resultado.Ok();

        return Resultado.Falha(await LerMensagem(resposta, cancellationToken));
    }

    public static async Task<Resultado<TValor>> Enviar<TValor>(
        this HttpClient httpClient,
        HttpMethod metodo,
        string url,
        object? corpo,
        CancellationToken cancellationToken)
    {
        using var resposta = await Requisitar(httpClient, metodo, url, corpo, cancellationToken);

        if (!resposta.IsSuccessStatusCode)
            return Resultado<TValor>.Falha(await LerMensagem(resposta, cancellationToken));

        var valor = await resposta.Content.ReadFromJsonAsync<TValor>(OpcoesJson, cancellationToken)
            ?? throw new InvalidOperationException($"A API respondeu 200 sem corpo em {metodo} {url}.");

        return Resultado<TValor>.Ok(valor);
    }

    // Para leitura simples, onde 404 significa ausência e não erro de negócio.
    public static async Task<TValor?> BuscarOuNulo<TValor>(
        this HttpClient httpClient,
        string url,
        CancellationToken cancellationToken)
    {
        using var resposta = await httpClient.GetAsync(url, cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.NotFound)
            return default;

        resposta.EnsureSuccessStatusCode();

        return await resposta.Content.ReadFromJsonAsync<TValor>(OpcoesJson, cancellationToken);
    }

    public static async Task<bool> Existe(
        this HttpClient httpClient,
        string url,
        CancellationToken cancellationToken)
    {
        using var resposta = await httpClient.GetAsync(url, cancellationToken);
        return resposta.IsSuccessStatusCode;
    }

    private static Task<HttpResponseMessage> Requisitar(
        HttpClient httpClient,
        HttpMethod metodo,
        string url,
        object? corpo,
        CancellationToken cancellationToken)
    {
        var requisicao = new HttpRequestMessage(metodo, url);

        if (corpo is not null)
            requisicao.Content = JsonContent.Create(corpo, options: OpcoesJson);

        return httpClient.SendAsync(requisicao, cancellationToken);
    }

    private static async Task<string> LerMensagem(HttpResponseMessage resposta, CancellationToken cancellationToken)
    {
        // 400 é falha de regra e traz mensagem. Outro status é defeito e estoura para o Site.
        if (resposta.StatusCode != HttpStatusCode.BadRequest)
            resposta.EnsureSuccessStatusCode();

        var erro = await resposta.Content.ReadFromJsonAsync<ErroApiModel>(OpcoesJson, cancellationToken);

        return string.IsNullOrWhiteSpace(erro?.Mensagem) ? "Dados inválidos." : erro.Mensagem;
    }
}
