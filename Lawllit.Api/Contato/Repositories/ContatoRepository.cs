using Npgsql;
using Dapper;
using Lawllit.Api.Common;
using Lawllit.Model.Contato;

namespace Lawllit.Api.Contato.Repositories;

public sealed class ContatoRepository(NpgsqlDataSource fonteDados) : IContatoRepository
{
    public async Task Criar(ContatoModel contato, CancellationToken cancellationToken)
    {
        const string consulta = """
            INSERT INTO CONTATO (CD_CONTATO, NM_CONTATO, TX_EMAIL, TX_MENSAGEM)
            VALUES (@CdContato, @NmContato, @TxEmail, @TxMensagem)
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(
            consulta,
            new { contato.CdContato, contato.NmContato, contato.TxEmail, contato.TxMensagem },
            cancellationToken: cancellationToken));
    }
}

#region Interfaces

public interface IContatoRepository
{
    Task Criar(ContatoModel contato, CancellationToken cancellationToken);
}

#endregion
