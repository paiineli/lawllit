using Dapper;
using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Models.Finance;
using Npgsql;

namespace Lawllit.Api.Finance.Repositories;

public class UserRepository(string connectionString) : IUserRepository
{
    private NpgsqlConnection CreateConnection() => new(connectionString);

    private const string SelectColumns = """
        "Id", "Name", "Email", "PasswordHash", "GoogleId", "EmailConfirmed",
        "EmailConfirmationToken", "EmailConfirmationTokenExpiry",
        "PasswordResetToken", "PasswordResetTokenExpiry",
        "CreatedAt", "Theme", "FontSize", "Language", "Currency", "IsOnboardingCompleted"
        """;

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM "Users"
            WHERE "Id" = @Id
            """;

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM "Users"
            WHERE "Email" = @Email
            """;

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM "Users"
            WHERE "GoogleId" = @GoogleId
            """;

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { GoogleId = googleId });
    }

    public async Task<User?> GetByConfirmationTokenAsync(string token)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM "Users"
            WHERE "EmailConfirmationToken" = @Token
            """;

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Token = token });
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM "Users"
            WHERE "PasswordResetToken" = @Token
            """;

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Token = token });
    }

    public async Task AddAsync(User user)
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

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        const string sql = """
            UPDATE "Users" SET
                "Name"                           = @Name,
                "Email"                          = @Email,
                "PasswordHash"                   = @PasswordHash,
                "GoogleId"                       = @GoogleId,
                "EmailConfirmed"                 = @EmailConfirmed,
                "EmailConfirmationToken"         = @EmailConfirmationToken,
                "EmailConfirmationTokenExpiry"   = @EmailConfirmationTokenExpiry,
                "PasswordResetToken"             = @PasswordResetToken,
                "PasswordResetTokenExpiry"       = @PasswordResetTokenExpiry,
                "Theme"                          = @Theme,
                "FontSize"                       = @FontSize,
                "Language"                       = @Language,
                "Currency"                       = @Currency,
                "IsOnboardingCompleted"          = @IsOnboardingCompleted
            WHERE "Id" = @Id
            """;

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, user);
    }

    public async Task DeleteAsync(Guid userId)
    {
        const string sql = """
            DELETE FROM "Users"
            WHERE "Id" = @UserId
            """;

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId });
    }
}
