using System.ComponentModel.DataAnnotations;

namespace Lawllit.Site.Models;

public sealed class ContatoViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(255, ErrorMessage = "O e-mail passou de 255 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escreva sua mensagem.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "A mensagem precisa ter entre 10 e 2000 caracteres.")]
    public string Mensagem { get; set; } = string.Empty;

    // Escondido por CSS. Robô preenche todo input, então vindo com valor o envio é descartado.
    public string? Site { get; set; }
}
