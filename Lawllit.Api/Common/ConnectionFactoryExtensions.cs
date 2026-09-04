using Npgsql;
using System.Data;

namespace Lawllit.Api.Common;

public static class ConnectionFactoryExtensions
{
    // Factory por chave em vez de string injetada no construtor do repositório.
    // Cada método abre e fecha a própria conexão, deixando o pool do Npgsql trabalhar.
    public static IServiceCollection AddConnectionFactory(this IServiceCollection services)
    {
        services.AddSingleton<Func<string, IDbConnection>>(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            return key =>
            {
                var connectionString = configuration.GetConnectionString(key);

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException(
                        $"Connection string '{key}' não configurada. Defina ConnectionStrings__{key} no ambiente.");

                return new NpgsqlConnection(connectionString);
            };
        });

        return services;
    }
}
