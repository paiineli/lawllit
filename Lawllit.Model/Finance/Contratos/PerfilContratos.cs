using Lawllit.Model.Common;

namespace Lawllit.Model.Finance.Contratos;

public sealed class PerfilModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MembroDesde { get; set; }
    public string Tema { get; set; } = "dark";
    public string TamanhoFonte { get; set; } = "normal";
}

public sealed class AlterarNomeModel
{
    public string Nome { get; set; } = string.Empty;

    public string? Validar()
        => Validacao.Tamanho(Nome, 1, 100) ? null : "Informe um nome de até 100 caracteres.";
}

public sealed class AlterarEmailModel
{
    public string Email { get; set; } = string.Empty;
    public string? Senha { get; set; }

    public string? Validar()
        => Validacao.Email(Email) ? null : "E-mail inválido.";
}

public sealed class AlterarSenhaModel
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;

    public string? Validar()
        => (NovaSenha?.Length ?? 0) < Validacao.SenhaMinima ? "A senha precisa ter pelo menos 6 caracteres." : null;
}

public sealed class PreferenciaModel
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}

public sealed class ExcluirContaModel
{
    public string? Senha { get; set; }
}
