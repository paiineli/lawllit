using Dapper;
using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Models.Finance;
using Npgsql;

namespace Lawllit.Api.Finance.Repositories;

public class CategoryRepository(string connectionString) : ICategoryRepository
{
    private NpgsqlConnection CreateConnection() => new(connectionString);

    public async Task<List<Category>> GetAllByUserAsync(Guid userId)
    {
        const string sql = """
            SELECT "Id", "Name", "Type", "UserId"
            FROM "Categories"
            WHERE "UserId" = @UserId
            ORDER BY "Type" ASC, "Name" ASC
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<Category>(sql, new { UserId = userId });
        return result.ToList();
    }

    public async Task<Category?> GetByIdAsync(Guid userId, Guid id)
    {
        const string sql = """
            SELECT "Id", "Name", "Type", "UserId"
            FROM "Categories"
            WHERE "UserId" = @UserId
              AND "Id" = @Id
            """;

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { UserId = userId, Id = id });
    }

    public async Task<bool> ExistsAsync(Guid userId, string name, TransactionType type, Guid? excludeId = null)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM "Categories"
            WHERE "UserId"    = @UserId
              AND "Type"      = @Type
              AND LOWER("Name") = LOWER(@Name)
              AND (@ExcludeId IS NULL OR "Id" != @ExcludeId)
            """;

        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, Name = name, Type = (int)type, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<bool> HasTransactionsAsync(Guid userId, Guid categoryId)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM "Transactions"
            WHERE "UserId"     = @UserId
              AND "CategoryId" = @CategoryId
            """;

        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, CategoryId = categoryId });
        return count > 0;
    }

    public async Task AddAsync(Category category)
    {
        const string sql = """
            INSERT INTO "Categories" ("Id", "Name", "Type", "UserId")
            VALUES (@Id, @Name, @Type, @UserId)
            """;

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new { category.Id, category.Name, Type = (int)category.Type, category.UserId });
    }

    public async Task UpdateAsync(Category category)
    {
        const string sql = """
            UPDATE "Categories" SET
                "Name" = @Name,
                "Type" = @Type
            WHERE "Id"     = @Id
              AND "UserId" = @UserId
            """;

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new { category.Name, Type = (int)category.Type, category.Id, category.UserId });
    }

    public async Task DeleteAsync(Guid categoryId)
    {
        const string sql = """
            DELETE FROM "Categories"
            WHERE "Id" = @CategoryId
            """;

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new { CategoryId = categoryId });
    }
}
