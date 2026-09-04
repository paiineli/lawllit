namespace Lawllit.Model.Finance.Contracts;

public sealed class LoginMOD
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// O Site manda o próprio molde da URL porque só ele conhece as rotas dele.
// A API só troca o {token} pelo valor gerado e monta o link do e-mail.
public sealed class RegisterMOD
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string ConfirmationUrlTemplate { get; set; } = string.Empty;
}

public sealed class ForgotPasswordMOD
{
    public string Email { get; set; } = string.Empty;
    public string ResetUrlTemplate { get; set; } = string.Empty;
}

public sealed class ResetPasswordMOD
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class GoogleUserMOD
{
    public string GoogleId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

// Retorno de todo fluxo que autentica. O Token é o JWT que o Site guarda no cookie
// e devolve para a API nas chamadas seguintes, carregando o Id do usuário.
public sealed class AuthResultMOD
{
    public UserMOD User { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
