namespace Lawllit.Api.Common;

public static class ConfiguracaoJwt
{
    public const string Emissor = "lawllit.api";
    public const string Audiencia = "lawllit.site";
    public const string ClaimAssunto = "sub";

    // O cookie do finance vence junto, pela claim api_token_expira, então só existe um prazo.
    public static readonly TimeSpan Validade = TimeSpan.FromDays(7);

    public static string LerChaveSecreta(IConfiguration configuration)
        => configuration["Jwt:SecretKey"]
           ?? throw new InvalidOperationException("Jwt__SecretKey não configurada no ambiente.");
}
