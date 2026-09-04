namespace Lawllit.Api.Common;

public static class JwtSettings
{
    public const string Issuer = "lawllit.api";
    public const string Audience = "lawllit.site";
    public const string SubjectClaim = "sub";

    // Mesma validade do cookie do Site, para os dois expirarem juntos e o usuário
    // não ficar com sessão viva e token morto.
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    public static string ReadSecretKey(IConfiguration configuration)
        => configuration["Jwt:SecretKey"]
           ?? throw new InvalidOperationException("Jwt__SecretKey não configurada no ambiente.");
}

public static class JwtRegisteredClaimNames
{
    public const string Subject = "sub";
}
