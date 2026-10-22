namespace Lawllit.Model.Finance.Contratos;

public sealed class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

// O molde da URL vem do Site porque só ele conhece as rotas dele. A API só troca o {token}.
public sealed class CadastroModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string MoldeUrlConfirmacao { get; set; } = string.Empty;
}

public sealed class EsqueciSenhaModel
{
    public string Email { get; set; } = string.Empty;
    public string MoldeUrlRedefinicao { get; set; } = string.Empty;
}

public sealed class RedefinirSenhaModel
{
    public string Token { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}

// Só o que o Site põe no cookie. Senha e tokens de e-mail nunca saem da API.
public sealed class UsuarioSessaoModel
{
    public Guid Codigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Tema { get; set; } = string.Empty;
    public string TamanhoFonte { get; set; } = string.Empty;

    public static UsuarioSessaoModel De(UsuarioModel usuario) => new()
    {
        Codigo = usuario.CdUsuario,
        Nome = usuario.NmUsuario,
        Email = usuario.TxEmail,
        Tema = usuario.TxTema,
        TamanhoFonte = usuario.TxTamanhoFonte,
    };
}

// O Token é o JWT que o Site guarda no cookie e devolve à API com o código do usuário.
public sealed class AutenticacaoModel
{
    public UsuarioSessaoModel Usuario { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
