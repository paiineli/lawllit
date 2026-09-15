using Npgsql;
using Dapper;
using Lawllit.Api.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;

namespace Lawllit.Api.Finance.Repositories;

public sealed class CategoriaRepository(NpgsqlDataSource fonteDados) : ICategoriaRepository
{
    private const string Colunas = """
        CD_CATEGORIA, NM_CATEGORIA, TX_TIPO, CD_USUARIO, SN_ATIVO, DT_CADASTRO
        """;

    public async Task<List<CategoriaModel>> Listar(Guid cdUsuario, CategoriaFiltroModel filtro, CancellationToken cancellationToken)
    {
        var consulta = $"""
            SELECT {Colunas}
            FROM CATEGORIA
            WHERE 1 = 1
              AND CD_USUARIO = @CdUsuario
              AND SN_ATIVO   = 'S'
            """;

        var parametros = new DynamicParameters();
        parametros.Add("CdUsuario", cdUsuario);

        if (filtro.Tipo.HasValue)
        {
            consulta += """

                  AND TX_TIPO = @TxTipo
                """;
            parametros.Add("TxTipo", filtro.Tipo.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            consulta += """

                  AND NM_CATEGORIA ILIKE @Busca
                """;
            parametros.Add("Busca", $"%{filtro.Busca.Trim()}%");
        }

        consulta += """

            ORDER BY TX_TIPO ASC, NM_CATEGORIA ASC
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        var categorias = await conexao.QueryAsync<CategoriaModel>(
            new CommandDefinition(consulta, parametros, cancellationToken: cancellationToken));

        return categorias.AsList();
    }

    public async Task<CategoriaModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdCategoria, CancellationToken cancellationToken)
    {
        var consulta = $"""
            SELECT {Colunas}
            FROM CATEGORIA
            WHERE CD_USUARIO   = @CdUsuario
              AND CD_CATEGORIA = @CdCategoria
              AND SN_ATIVO     = 'S'
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        return await conexao.QueryFirstOrDefaultAsync<CategoriaModel>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, CdCategoria = cdCategoria },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> Existe(Guid cdUsuario, string nome, TipoTransacaoEnum tipo, Guid? cdIgnorar, CancellationToken cancellationToken)
    {
        const string consulta = """
            SELECT COUNT(1)
            FROM CATEGORIA
            WHERE CD_USUARIO         = @CdUsuario
              AND TX_TIPO            = @TxTipo
              AND LOWER(NM_CATEGORIA) = LOWER(@NmCategoria)
              AND SN_ATIVO           = 'S'
              AND (@CdIgnorar IS NULL OR CD_CATEGORIA != @CdIgnorar)
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        var total = await conexao.ExecuteScalarAsync<int>(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, NmCategoria = nome, TxTipo = tipo.ToString(), CdIgnorar = cdIgnorar },
            cancellationToken: cancellationToken));

        return total > 0;
    }

    public async Task Criar(CategoriaModel categoria, CancellationToken cancellationToken)
    {
        const string consulta = """
            INSERT INTO CATEGORIA (CD_CATEGORIA, NM_CATEGORIA, TX_TIPO, CD_USUARIO)
            VALUES (@CdCategoria, @NmCategoria, @TxTipo, @CdUsuario)
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(
            consulta,
            new
            {
                categoria.CdCategoria,
                categoria.NmCategoria,
                TxTipo = categoria.TxTipo.ToString(),
                categoria.CdUsuario,
            },
            cancellationToken: cancellationToken));
    }

    public async Task Alterar(CategoriaModel categoria, CancellationToken cancellationToken)
    {
        const string consulta = """
            UPDATE CATEGORIA SET
                NM_CATEGORIA = @NmCategoria,
                TX_TIPO      = @TxTipo
            WHERE CD_CATEGORIA = @CdCategoria
              AND CD_USUARIO   = @CdUsuario
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(
            consulta,
            new
            {
                categoria.NmCategoria,
                TxTipo = categoria.TxTipo.ToString(),
                categoria.CdCategoria,
                categoria.CdUsuario,
            },
            cancellationToken: cancellationToken));
    }

    public async Task AlternarAtivo(Guid cdUsuario, Guid cdCategoria, string snAtivo, CancellationToken cancellationToken)
    {
        const string consulta = """
            UPDATE CATEGORIA SET
                SN_ATIVO = @SnAtivo
            WHERE CD_CATEGORIA = @CdCategoria
              AND CD_USUARIO   = @CdUsuario
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(
            consulta,
            new { CdUsuario = cdUsuario, CdCategoria = cdCategoria, SnAtivo = snAtivo },
            cancellationToken: cancellationToken));
    }
}

#region Interfaces

public interface ICategoriaRepository
{
    Task<List<CategoriaModel>> Listar(Guid cdUsuario, CategoriaFiltroModel filtro, CancellationToken cancellationToken);
    Task<CategoriaModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdCategoria, CancellationToken cancellationToken);
    Task<bool> Existe(Guid cdUsuario, string nome, TipoTransacaoEnum tipo, Guid? cdIgnorar, CancellationToken cancellationToken);
    Task Criar(CategoriaModel categoria, CancellationToken cancellationToken);
    Task Alterar(CategoriaModel categoria, CancellationToken cancellationToken);
    Task AlternarAtivo(Guid cdUsuario, Guid cdCategoria, string snAtivo, CancellationToken cancellationToken);
}

#endregion
