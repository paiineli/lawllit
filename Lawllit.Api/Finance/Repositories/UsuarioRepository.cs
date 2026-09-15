using Npgsql;
using Dapper;
using Lawllit.Api.Common;
using Lawllit.Model.Finance;

namespace Lawllit.Api.Finance.Repositories;

public sealed class UsuarioRepository(NpgsqlDataSource fonteDados) : IUsuarioRepository
{
    private const string Colunas = """
        CD_USUARIO, NM_USUARIO, TX_EMAIL, TX_SENHA, SN_EMAIL_CONFIRMADO,
        TX_TOKEN_CONFIRMACAO, DT_EXPIRA_CONFIRMACAO,
        TX_TOKEN_SENHA, DT_EXPIRA_TOKEN_SENHA,
        TX_TEMA, TX_TAMANHO_FONTE, SN_ATIVO, DT_CADASTRO
        """;

    // Genérico e não object: o Dapper tipa o parâmetro pelo tipo declarado da propriedade.
    private async Task<UsuarioModel?> BuscarPorColuna<TValor>(string coluna, TValor valor, CancellationToken cancellationToken)
    {
        // A coluna vem de constante interna, nunca de entrada, então a interpolação é segura.
        var consulta = $"""
            SELECT {Colunas}
            FROM USUARIO
            WHERE {coluna} = @Valor
              AND SN_ATIVO = 'S'
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        return await conexao.QueryFirstOrDefaultAsync<UsuarioModel>(
            new CommandDefinition(consulta, new { Valor = valor }, cancellationToken: cancellationToken));
    }

    public Task<UsuarioModel?> BuscarPorCodigo(Guid cdUsuario, CancellationToken cancellationToken)
        => BuscarPorColuna("CD_USUARIO", cdUsuario, cancellationToken);

    public Task<UsuarioModel?> BuscarPorEmail(string email, CancellationToken cancellationToken)
        => BuscarPorColuna("TX_EMAIL", email, cancellationToken);

    public Task<UsuarioModel?> BuscarPorTokenConfirmacao(string token, CancellationToken cancellationToken)
        => BuscarPorColuna("TX_TOKEN_CONFIRMACAO", token, cancellationToken);

    public Task<UsuarioModel?> BuscarPorTokenSenha(string token, CancellationToken cancellationToken)
        => BuscarPorColuna("TX_TOKEN_SENHA", token, cancellationToken);

    public async Task Criar(UsuarioModel usuario, CancellationToken cancellationToken)
    {
        const string consulta = """
            INSERT INTO USUARIO (
                CD_USUARIO, NM_USUARIO, TX_EMAIL, TX_SENHA, SN_EMAIL_CONFIRMADO,
                TX_TOKEN_CONFIRMACAO, DT_EXPIRA_CONFIRMACAO,
                TX_TOKEN_SENHA, DT_EXPIRA_TOKEN_SENHA,
                TX_TEMA, TX_TAMANHO_FONTE, SN_ATIVO, DT_CADASTRO
            ) VALUES (
                @CdUsuario, @NmUsuario, @TxEmail, @TxSenha, @SnEmailConfirmado,
                @TxTokenConfirmacao, @DtExpiraConfirmacao,
                @TxTokenSenha, @DtExpiraTokenSenha,
                @TxTema, @TxTamanhoFonte, @SnAtivo, @DtCadastro
            )
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(consulta, usuario, cancellationToken: cancellationToken));
    }

    public async Task Alterar(UsuarioModel usuario, CancellationToken cancellationToken)
    {
        const string consulta = """
            UPDATE USUARIO SET
                NM_USUARIO            = @NmUsuario,
                TX_EMAIL              = @TxEmail,
                TX_SENHA              = @TxSenha,
                SN_EMAIL_CONFIRMADO   = @SnEmailConfirmado,
                TX_TOKEN_CONFIRMACAO  = @TxTokenConfirmacao,
                DT_EXPIRA_CONFIRMACAO = @DtExpiraConfirmacao,
                TX_TOKEN_SENHA        = @TxTokenSenha,
                DT_EXPIRA_TOKEN_SENHA = @DtExpiraTokenSenha,
                TX_TEMA               = @TxTema,
                TX_TAMANHO_FONTE      = @TxTamanhoFonte
            WHERE CD_USUARIO = @CdUsuario
            """;

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await conexao.ExecuteAsync(new CommandDefinition(consulta, usuario, cancellationToken: cancellationToken));
    }

    // Exclusão física, única do sistema, porque a LGPD dá direito à eliminação de verdade.
    public async Task Excluir(Guid cdUsuario, CancellationToken cancellationToken)
    {
        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        conexao.Open();
        using var transacao = conexao.BeginTransaction();

        foreach (var consulta in new[]
                 {
                     "DELETE FROM TRANSACAO WHERE CD_USUARIO = @CdUsuario",
                     "DELETE FROM CATEGORIA WHERE CD_USUARIO = @CdUsuario",
                     "DELETE FROM USUARIO   WHERE CD_USUARIO = @CdUsuario",
                 })
        {
            await conexao.ExecuteAsync(new CommandDefinition(
                consulta, new { CdUsuario = cdUsuario }, transacao, cancellationToken: cancellationToken));
        }

        transacao.Commit();
    }
}

#region Interfaces

public interface IUsuarioRepository
{
    Task<UsuarioModel?> BuscarPorCodigo(Guid cdUsuario, CancellationToken cancellationToken);
    Task<UsuarioModel?> BuscarPorEmail(string email, CancellationToken cancellationToken);
    Task<UsuarioModel?> BuscarPorTokenConfirmacao(string token, CancellationToken cancellationToken);
    Task<UsuarioModel?> BuscarPorTokenSenha(string token, CancellationToken cancellationToken);
    Task Criar(UsuarioModel usuario, CancellationToken cancellationToken);
    Task Alterar(UsuarioModel usuario, CancellationToken cancellationToken);
    Task Excluir(Guid cdUsuario, CancellationToken cancellationToken);
}

#endregion
