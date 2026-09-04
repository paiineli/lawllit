using Dapper;
using Lawllit.Api.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using System.Data;

namespace Lawllit.Api.Finance.Repositories;

public sealed class CategoryREP(Func<string, IDbConnection> connectionFactory) : ICategoryREP
{
    private const string SelectColumns = """
        "Id", "Name", "Type", "UserId"
        """;

    public async Task<List<CategoryMOD>> GetFilteredAsync(Guid userId, CategoryFilterMOD filter, CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM "Categories"
            WHERE 1 = 1
              AND "UserId" = @UserId
            """;

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (filter.Type.HasValue)
        {
            sql += """

                  AND "Type" = @Type
                """;
            parameters.Add("Type", (int)filter.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sql += """

                  AND "Name" ILIKE @Search
                """;
            parameters.Add("Search", $"%{filter.Search.Trim()}%");
        }

        sql += """

            ORDER BY "Type" ASC, "Name" ASC
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        var categories = await connection.QueryAsync<CategoryMOD>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        return categories.AsList();
    }

    public async Task<CategoryMOD?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM "Categories"
            WHERE "UserId" = @UserId
              AND "Id"     = @Id
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        return await connection.QueryFirstOrDefaultAsync<CategoryMOD>(
            new CommandDefinition(sql, new { UserId = userId, Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsAsync(Guid userId, string name, TransactionTypeEnum type, Guid? excludeId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM "Categories"
            WHERE "UserId"      = @UserId
              AND "Type"        = @Type
              AND LOWER("Name") = LOWER(@Name)
              AND (@ExcludeId IS NULL OR "Id" != @ExcludeId)
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { UserId = userId, Name = name, Type = (int)type, ExcludeId = excludeId },
            cancellationToken: cancellationToken));

        return total > 0;
    }

    public async Task<bool> HasTransactionsAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM "Transactions"
            WHERE "UserId"     = @UserId
              AND "CategoryId" = @CategoryId
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { UserId = userId, CategoryId = categoryId },
            cancellationToken: cancellationToken));

        return total > 0;
    }

    public async Task AddAsync(CategoryMOD category, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO "Categories" ("Id", "Name", "Type", "UserId")
            VALUES (@Id, @Name, @Type, @UserId)
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { category.Id, category.Name, Type = (int)category.Type, category.UserId },
            cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(CategoryMOD category, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE "Categories" SET
                "Name" = @Name,
                "Type" = @Type
            WHERE "Id"     = @Id
              AND "UserId" = @UserId
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { category.Name, Type = (int)category.Type, category.Id, category.UserId },
            cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM "Categories"
            WHERE "Id"     = @CategoryId
              AND "UserId" = @UserId
            """;

        using var connection = connectionFactory(ConnectionKeys.Lawllit);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { UserId = userId, CategoryId = categoryId },
            cancellationToken: cancellationToken));
    }
}

#region Interfaces

public interface ICategoryREP
{
    Task<List<CategoryMOD>> GetFilteredAsync(Guid userId, CategoryFilterMOD filter, CancellationToken cancellationToken);
    Task<CategoryMOD?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid userId, string name, TransactionTypeEnum type, Guid? excludeId, CancellationToken cancellationToken);
    Task<bool> HasTransactionsAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken);
    Task AddAsync(CategoryMOD category, CancellationToken cancellationToken);
    Task UpdateAsync(CategoryMOD category, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken);
}

#endregion
