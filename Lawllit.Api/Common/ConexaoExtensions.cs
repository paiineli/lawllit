using Npgsql;

namespace Lawllit.Api.Common;

public static class ConexaoExtensions
{
    public static IServiceCollection AddBanco(this IServiceCollection services, IConfiguration configuration)
    {
        var conexao = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings__DefaultConnection não configurada no ambiente.");

        services.AddSingleton(NpgsqlDataSource.Create(Normalizar(conexao)));

        return services;
    }

    // A Railway entrega a conexão como URI e o Npgsql espera chave e valor. Aceita as duas formas.
    private static string Normalizar(string conexao)
    {
        if (!conexao.StartsWith("postgres", StringComparison.OrdinalIgnoreCase) || !conexao.Contains("://"))
            return conexao;

        var uri = new Uri(conexao);
        var partes = uri.UserInfo.Split(':', 2);

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(partes[0]),
            Password = partes.Length > 1 ? Uri.UnescapeDataString(partes[1]) : null,
            SslMode = SslMode.Require,
        }.ConnectionString;
    }
}
