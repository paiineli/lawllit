using Lawllit.Api.Common;
using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;

namespace Lawllit.Api.Finance.Services;

public sealed class AutenticacaoService(
    IUsuarioRepository usuarioRepositorio,
    ICategoriaRepository categoriaRepositorio,
    IEmailService emailService,
    GeradorToken geradorToken) : IAutenticacaoService
{
    // Sem categoria não dá para lançar transação, então a conta nova já nasce com estas.
    private static readonly (string Nome, TipoTransacaoEnum Tipo)[] CategoriasPadrao =
    [
        ("Salário",                TipoTransacaoEnum.RECEITA),
        ("Outras receitas",        TipoTransacaoEnum.RECEITA),
        ("Moradia",                TipoTransacaoEnum.DESPESA),
        ("Alimentação",            TipoTransacaoEnum.DESPESA),
        ("Transporte",             TipoTransacaoEnum.DESPESA),
        ("Saúde",                  TipoTransacaoEnum.DESPESA),
        ("Lazer",                  TipoTransacaoEnum.DESPESA),
        ("Outras despesas",        TipoTransacaoEnum.DESPESA),
        ("Reserva de emergência",  TipoTransacaoEnum.INVESTIMENTO),
        ("Renda variável",         TipoTransacaoEnum.INVESTIMENTO),
    ];

    public async Task<Resultado<AutenticacaoModel>> Entrar(LoginModel login, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorEmail(login.Email.Trim().ToLower(), cancellationToken);

        if (usuario is null || !Senha.Confere(login.Senha, usuario.TxSenha))
            return Resultado<AutenticacaoModel>.Falha("E-mail ou senha incorretos.");

        if (!usuario.EmailConfirmado)
            return Resultado<AutenticacaoModel>.Falha("Confirme seu e-mail antes de entrar.");

        return Resultado<AutenticacaoModel>.Ok(MontarAutenticacao(usuario));
    }

    public async Task<Resultado> Cadastrar(CadastroModel cadastro, CancellationToken cancellationToken)
    {
        var email = cadastro.Email.Trim().ToLower();

        var existente = await usuarioRepositorio.BuscarPorEmail(email, cancellationToken);
        if (existente is not null)
            return Resultado.Falha("Este e-mail já está em uso.");

        var usuario = new UsuarioModel
        {
            CdUsuario = Guid.NewGuid(),
            NmUsuario = Textos.ParaTitulo(cadastro.Nome),
            TxEmail = email,
            TxSenha = Senha.Gerar(cadastro.Senha),
            SnEmailConfirmado = Constantes.Nao,
            TxTokenConfirmacao = Guid.NewGuid().ToString("N"),
            DtExpiraConfirmacao = DateTime.UtcNow.AddHours(24),
            SnAtivo = Constantes.Sim,
            DtCadastro = DateTime.UtcNow,
        };

        await usuarioRepositorio.Criar(usuario, cancellationToken);

        foreach (var (nome, tipo) in CategoriasPadrao)
        {
            await categoriaRepositorio.Criar(new CategoriaModel
            {
                CdCategoria = Guid.NewGuid(),
                NmCategoria = nome,
                TxTipo = tipo,
                CdUsuario = usuario.CdUsuario,
            }, cancellationToken);
        }

        await emailService.EnviarConfirmacao(
            usuario,
            cadastro.MoldeUrlConfirmacao.Replace("{token}", usuario.TxTokenConfirmacao),
            cancellationToken);

        return Resultado.Ok();
    }

    public async Task<Resultado<AutenticacaoModel>> ConfirmarEmail(string token, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorTokenConfirmacao(token, cancellationToken);

        if (usuario is null || usuario.DtExpiraConfirmacao < DateTime.UtcNow)
            return Resultado<AutenticacaoModel>.Falha("Link inválido ou expirado.");

        usuario.SnEmailConfirmado = Constantes.Sim;
        usuario.TxTokenConfirmacao = null;
        usuario.DtExpiraConfirmacao = null;
        await usuarioRepositorio.Alterar(usuario, cancellationToken);

        return Resultado<AutenticacaoModel>.Ok(MontarAutenticacao(usuario));
    }

    public async Task<bool> TokenSenhaValido(string token, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorTokenSenha(token, cancellationToken);
        return usuario is not null && usuario.DtExpiraTokenSenha >= DateTime.UtcNow;
    }

    public async Task<Resultado> EsqueciSenha(EsqueciSenhaModel esqueciSenha, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorEmail(esqueciSenha.Email.Trim().ToLower(), cancellationToken);

        // Mensagem genérica nos dois casos, para a tela não virar consulta de quem tem cadastro.
        if (usuario is null || !usuario.EmailConfirmado)
            return Resultado.Falha("Não encontrado.");

        usuario.TxTokenSenha = Guid.NewGuid().ToString("N");
        usuario.DtExpiraTokenSenha = DateTime.UtcNow.AddHours(1);
        await usuarioRepositorio.Alterar(usuario, cancellationToken);

        await emailService.EnviarRedefinicaoSenha(
            usuario,
            esqueciSenha.MoldeUrlRedefinicao.Replace("{token}", usuario.TxTokenSenha),
            cancellationToken);

        return Resultado.Ok();
    }

    public async Task<Resultado> RedefinirSenha(RedefinirSenhaModel redefinir, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorTokenSenha(redefinir.Token, cancellationToken);

        if (usuario is null || usuario.DtExpiraTokenSenha < DateTime.UtcNow)
            return Resultado.Falha("Link inválido ou expirado.");

        usuario.TxSenha = Senha.Gerar(redefinir.NovaSenha);
        usuario.TxTokenSenha = null;
        usuario.DtExpiraTokenSenha = null;
        await usuarioRepositorio.Alterar(usuario, cancellationToken);

        return Resultado.Ok();
    }

    private AutenticacaoModel MontarAutenticacao(UsuarioModel usuario)
    {
        var token = geradorToken.Gerar(usuario);

        return new AutenticacaoModel
        {
            Usuario = usuario,
            Token = token.Token,
            ExpiraEm = token.ExpiraEm,
        };
    }
}

#region Interfaces

public interface IAutenticacaoService
{
    Task<Resultado<AutenticacaoModel>> Entrar(LoginModel login, CancellationToken cancellationToken);
    Task<Resultado> Cadastrar(CadastroModel cadastro, CancellationToken cancellationToken);
    Task<Resultado<AutenticacaoModel>> ConfirmarEmail(string token, CancellationToken cancellationToken);
    Task<bool> TokenSenhaValido(string token, CancellationToken cancellationToken);
    Task<Resultado> EsqueciSenha(EsqueciSenhaModel esqueciSenha, CancellationToken cancellationToken);
    Task<Resultado> RedefinirSenha(RedefinirSenhaModel redefinir, CancellationToken cancellationToken);
}

#endregion
