namespace Lawllit.Site.Common;

// Saem do host da requisição, então trocar de domínio não mexe em view nenhuma.
public static class SeoUrls
{
    private static string Origem(HttpRequest request)
        => $"{request.Scheme}://{request.Host}{request.PathBase}";

    public static string Absoluta(HttpRequest request, string caminho)
        => Origem(request) + caminho;

    // Sem a query de propósito: filtro de tela não é página nova, seria endereço duplicado.
    public static string Canonica(HttpRequest request)
        => Absoluta(request, request.Path);
}
