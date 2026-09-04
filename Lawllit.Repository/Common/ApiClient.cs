using Lawllit.Model.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lawllit.Repository.Common;

public static class ApiClient
{
    public const string HttpClientName = "LawllitApi";

    // Mesmo contrato de serialização das duas pontas, camelCase com enum em texto.
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<Result> SendAsync(
        this HttpClient httpClient,
        HttpMethod method,
        string url,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendRequestAsync(httpClient, method, url, body, cancellationToken);

        if (response.IsSuccessStatusCode)
            return Result.Success();

        return Result.Failure(await ReadErrorKeyAsync(response, cancellationToken));
    }

    public static async Task<Result<TValue>> SendAsync<TValue>(
        this HttpClient httpClient,
        HttpMethod method,
        string url,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendRequestAsync(httpClient, method, url, body, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result<TValue>.Failure(await ReadErrorKeyAsync(response, cancellationToken));

        var value = await response.Content.ReadFromJsonAsync<TValue>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException($"A API respondeu 200 sem corpo em {method} {url}.");

        return Result<TValue>.Success(value);
    }

    // Para leitura simples, onde 404 significa ausência e não erro de negócio.
    public static async Task<TValue?> GetOrDefaultAsync<TValue>(
        this HttpClient httpClient,
        string url,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TValue>(JsonOptions, cancellationToken);
    }

    public static async Task<bool> ExistsAsync(
        this HttpClient httpClient,
        string url,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(url, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private static Task<HttpResponseMessage> SendRequestAsync(
        HttpClient httpClient,
        HttpMethod method,
        string url,
        object? body,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, url);

        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        return httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task<string> ReadErrorKeyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        // 400 é falha de regra e sempre traz a ErrorKey. Qualquer outro status é defeito,
        // então estoura para o handler de exceção do Site em vez de virar mensagem amigável.
        if (response.StatusCode != HttpStatusCode.BadRequest)
            response.EnsureSuccessStatusCode();

        var error = await response.Content.ReadFromJsonAsync<ApiErrorMOD>(JsonOptions, cancellationToken);

        return string.IsNullOrWhiteSpace(error?.ErrorKey) ? "Msg_DataInvalid" : error.ErrorKey;
    }
}
