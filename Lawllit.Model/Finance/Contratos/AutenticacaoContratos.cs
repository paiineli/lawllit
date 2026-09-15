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

// O Token é o JWT que o Site guarda no cookie e devolve à API com o código do usuário.
public sealed class AutenticacaoModel
{
    public UsuarioModel Usuario { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
