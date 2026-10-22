using Lawllit.Model.Common;

namespace Lawllit.Model.Contato.Contratos;

public sealed class EnviarMensagemModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;

    public string? Validar()
    {
        if (!Validacao.Tamanho(Nome, 1, 100)) return "Informe um nome de até 100 caracteres.";
        if (!Validacao.Email(Email)) return "E-mail inválido.";
        if (!Validacao.Tamanho(Mensagem, 10, 2000)) return "A mensagem precisa ter entre 10 e 2000 caracteres.";
        return null;
    }
}
