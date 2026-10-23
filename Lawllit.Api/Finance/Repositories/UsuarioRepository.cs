using Npgsql;
using Dapper;
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

    // Usuário e categorias padrão entram juntos: conta sem categoria não consegue lançar nada.
    // Devolve falso quando o e-mail já existe, o que acontece com dois cadastros simultâneos.
    public async Task<bool> Criar(UsuarioModel usuario, IEnumerable<CategoriaModel> categorias, CancellationToken cancellationToken)
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
        await using var transacao = await conexao.BeginTransactionAsync(cancellationToken);

        try
        {
            await conexao.ExecuteAsync(new CommandDefinition(consulta, usuario, transacao, cancellationToken: cancellationToken));

            foreach (var categoria in categorias)
            {
                await conexao.ExecuteAsync(new CommandDefinition(
                    CategoriaRepository.ComandoCriar, CategoriaRepository.ParametrosCriar(categoria), transacao, cancellationToken: cancellationToken));
            }
        }
        catch (PostgresException erro) when (erro.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return false;
        }

        await transacao.CommitAsync(cancellationToken);
        return true;
    }

    // Cada alteração mexe só nas próprias colunas. Regravar a linha inteira perdia o que outra requisição mudou no meio.
    private async Task<bool> AlterarColunas(Guid cdUsuario, string colunas, object parametros, CancellationToken cancellationToken, string condicao = "")
    {
        // As colunas e a condição vêm de constantes desta classe, nunca de entrada, então a interpolação é segura.
        var consulta = $"UPDATE USUARIO SET {colunas} WHERE CD_USUARIO = @CdUsuario AND SN_ATIVO = 'S' {condicao}";

        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        var argumentos = new DynamicParameters(parametros);
        argumentos.Add("CdUsuario", cdUsuario);

        return await conexao.ExecuteAsync(new CommandDefinition(consulta, argumentos, cancellationToken: cancellationToken)) > 0;
    }

    public Task<bool> AlterarNome(Guid cdUsuario, string nome, CancellationToken cancellationToken)
        => AlterarColunas(cdUsuario, "NM_USUARIO = @Nome", new { Nome = nome }, cancellationToken);

    // Falso também quando outro cadastro pegou o e-mail entre a checagem e a gravação.
    public async Task<bool> AlterarEmail(Guid cdUsuario, string email, CancellationToken cancellationToken)
    {
        try
        {
            return await AlterarColunas(cdUsuario, "TX_EMAIL = @Email", new { Email = email }, cancellationToken);
        }
        catch (PostgresException erro) when (erro.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return false;
        }
    }

    // Senha nova invalida qualquer link de redefinição que ainda estivesse aberto.
    public Task<bool> AlterarSenha(Guid cdUsuario, string senhaHash, CancellationToken cancellationToken)
        => AlterarColunas(cdUsuario, "TX_SENHA = @Senha, TX_TOKEN_SENHA = NULL, DT_EXPIRA_TOKEN_SENHA = NULL", new { Senha = senhaHash }, cancellationToken);

    public Task<bool> AlterarPreferencias(Guid cdUsuario, string tema, string tamanhoFonte, CancellationToken cancellationToken)
        => AlterarColunas(cdUsuario, "TX_TEMA = @Tema, TX_TAMANHO_FONTE = @TamanhoFonte", new { Tema = tema, TamanhoFonte = tamanhoFonte }, cancellationToken);

    public Task<bool> ConfirmarEmail(Guid cdUsuario, CancellationToken cancellationToken)
        => AlterarColunas(cdUsuario, "SN_EMAIL_CONFIRMADO = 'S', TX_TOKEN_CONFIRMACAO = NULL, DT_EXPIRA_CONFIRMACAO = NULL", new { }, cancellationToken);

    public Task<bool> GravarTokenSenha(Guid cdUsuario, string token, DateTime expiraEm, CancellationToken cancellationToken)
        => AlterarColunas(cdUsuario, "TX_TOKEN_SENHA = @Token, DT_EXPIRA_TOKEN_SENHA = @ExpiraEm", new { Token = token, ExpiraEm = expiraEm }, cancellationToken);

    // Só enquanto a conta não foi confirmada: depois disso o recadastro não pode mais trocar a senha de ninguém.
    public Task<bool> RecadastrarPendente(UsuarioModel usuario, CancellationToken cancellationToken)
        => AlterarColunas(
            usuario.CdUsuario,
            "NM_USUARIO = @NmUsuario, TX_SENHA = @TxSenha, TX_TOKEN_CONFIRMACAO = @TxTokenConfirmacao, DT_EXPIRA_CONFIRMACAO = @DtExpiraConfirmacao",
            new { usuario.NmUsuario, usuario.TxSenha, usuario.TxTokenConfirmacao, usuario.DtExpiraConfirmacao },
            cancellationToken,
            condicao: "AND SN_EMAIL_CONFIRMADO = 'N'");

    // Exclusão física, única do sistema, porque a LGPD dá direito à eliminação de verdade.
    public async Task Excluir(Guid cdUsuario, CancellationToken cancellationToken)
    {
        // O OpenConnectionAsync já entrega a conexão aberta. Um Open() aqui lançava e a exclusão nunca rodava.
        await using var conexao = await fonteDados.OpenConnectionAsync(cancellationToken);
        await using var transacao = await conexao.BeginTransactionAsync(cancellationToken);

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

        await transacao.CommitAsync(cancellationToken);
    }
}

#region Interfaces

public interface IUsuarioRepository
{
    Task<UsuarioModel?> BuscarPorCodigo(Guid cdUsuario, CancellationToken cancellationToken);
    Task<UsuarioModel?> BuscarPorEmail(string email, CancellationToken cancellationToken);
    Task<UsuarioModel?> BuscarPorTokenConfirmacao(string token, CancellationToken cancellationToken);
    Task<UsuarioModel?> BuscarPorTokenSenha(string token, CancellationToken cancellationToken);
    Task<bool> Criar(UsuarioModel usuario, IEnumerable<CategoriaModel> categorias, CancellationToken cancellationToken);
    Task<bool> AlterarNome(Guid cdUsuario, string nome, CancellationToken cancellationToken);
    Task<bool> AlterarEmail(Guid cdUsuario, string email, CancellationToken cancellationToken);
    Task<bool> AlterarSenha(Guid cdUsuario, string senhaHash, CancellationToken cancellationToken);
    Task<bool> AlterarPreferencias(Guid cdUsuario, string tema, string tamanhoFonte, CancellationToken cancellationToken);
    Task<bool> ConfirmarEmail(Guid cdUsuario, CancellationToken cancellationToken);
    Task<bool> GravarTokenSenha(Guid cdUsuario, string token, DateTime expiraEm, CancellationToken cancellationToken);
    Task<bool> RecadastrarPendente(UsuarioModel usuario, CancellationToken cancellationToken);
    Task Excluir(Guid cdUsuario, CancellationToken cancellationToken);
}

#endregion
