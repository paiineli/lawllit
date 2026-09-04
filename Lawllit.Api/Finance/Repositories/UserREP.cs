using Dapper;
using Lawllit.Api.Common;
using Lawllit.Model.Finance;
using System.Data;

namespace Lawllit.Api.Finance.Repositories;

public sealed class UserREP(Func<string, IDbConnection> connectionFactory) : IUserREP
{
    private const string SelectColumns = """
        "Id", "Name", "Email", "PasswordHash", "GoogleId", "EmailConfirmed",
        "EmailConfirmationToken", "EmailConfirmationTokenExpiry",
        "PasswordResetToken", "PasswordResetTokenExpiry",
        "CreatedAt", "Theme", "FontSize", "Language", "Currency", "IsOnboardingCompleted"
        """;

    private async Task<UserMOD?> GetByColumnAsync(string column, object value, CancellationToken cancellationToken)
    {
        // A coluna vem sempre de constante interna desta classe, nunca de entrada
        // do usuário, então a interpolação aqui não abre espaço para injeção.
        var sql = $"""
            SELECT {SelectColumns}
            FROM "Users"
            WHERE "{column}" = @Value
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        return await connection.QueryFirstOrDefaultAsync<UserMOD>(
            new CommandDefinition(sql, new { Value = value }, cancellationToken: cancellationToken));
    }

    public Task<UserMOD?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => GetByColumnAsync("Id", id, cancellationToken);

    public Task<UserMOD?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => GetByColumnAsync("Email", email, cancellationToken);

    public Task<UserMOD?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken)
        => GetByColumnAsync("GoogleId", googleId, cancellationToken);

    public Task<UserMOD?> GetByConfirmationTokenAsync(string token, CancellationToken cancellationToken)
        => GetByColumnAsync("EmailConfirmationToken", token, cancellationToken);

    public Task<UserMOD?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
        => GetByColumnAsync("PasswordResetToken", token, cancellationToken);

    public async Task AddAsync(UserMOD user, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO "Users" (
                "Id", "Name", "Email", "PasswordHash", "GoogleId", "EmailConfirmed",
                "EmailConfirmationToken", "EmailConfirmationTokenExpiry",
                "PasswordResetToken", "PasswordResetTokenExpiry",
                "CreatedAt", "Theme", "FontSize", "Language", "Currency", "IsOnboardingCompleted"
            ) VALUES (
                @Id, @Name, @Email, @PasswordHash, @GoogleId, @EmailConfirmed,
                @EmailConfirmationToken, @EmailConfirmationTokenExpiry,
                @PasswordResetToken, @PasswordResetTokenExpiry,
                @CreatedAt, @Theme, @FontSize, @Language, @Currency, @IsOnboardingCompleted
            )
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(UserMOD user, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE "Users" SET
                "Name"                         = @Name,
                "Email"                        = @Email,
                "PasswordHash"                 = @PasswordHash,
                "GoogleId"                     = @GoogleId,
                "EmailConfirmed"               = @EmailConfirmed,
                "EmailConfirmationToken"       = @EmailConfirmationToken,
                "EmailConfirmationTokenExpiry" = @EmailConfirmationTokenExpiry,
                "PasswordResetToken"           = @PasswordResetToken,
                "PasswordResetTokenExpiry"     = @PasswordResetTokenExpiry,
                "Theme"                        = @Theme,
                "FontSize"                     = @FontSize,
                "Language"                     = @Language,
                "Currency"                     = @Currency,
                "IsOnboardingCompleted"        = @IsOnboardingCompleted
            WHERE "Id" = @Id
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM "Users"
            WHERE "Id" = @UserId
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: cancellationToken));
    }
}

#region Interfaces

public interface IUserREP
{
    Task<UserMOD?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserMOD?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserMOD?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken);
    Task<UserMOD?> GetByConfirmationTokenAsync(string token, CancellationToken cancellationToken);
    Task<UserMOD?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken);
    Task AddAsync(UserMOD user, CancellationToken cancellationToken);
    Task UpdateAsync(UserMOD user, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}

#endregion
