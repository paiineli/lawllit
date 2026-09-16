using Lawllit.Model.Finance.Contratos;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Lawllit.Finance.Site.Models;

public sealed class PerfilViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool TemSenha { get; set; }
    public DateTime MembroDesde { get; set; }
    public string AbaAtiva { get; set; } = "info";
    public string Tema { get; set; } = "dark";
    public string TamanhoFonte { get; set; } = "normal";

    // "Set. 2025". O .NET abrevia o mês com ponto em alguns e sem em outros, daí o ajuste.
    public string MembroDesdeTexto
    {
        get
        {
            var mesCru = MembroDesde.ToString("MMM", CultureInfo.CurrentCulture);
            var mes = char.ToUpper(mesCru[0]) + mesCru[1..].TrimEnd('.') + ".";
            return $"{mes} {MembroDesde.Year}";
        }
    }

    public static PerfilViewModel De(PerfilModel perfil, string? aba) => new()
    {
        Nome = perfil.Nome,
        Email = perfil.Email,
        TemSenha = perfil.TemSenha,
        MembroDesde = perfil.MembroDesde,
        AbaAtiva = aba ?? "info",
        Tema = perfil.Tema,
        TamanhoFonte = perfil.TamanhoFonte,
    };
}

public sealed class AlterarNomeViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
}

public sealed class AlterarEmailViewModel
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    public string? Senha { get; set; }
}

public sealed class AlterarSenhaViewModel
{
    [Required(ErrorMessage = "Senha é obrigatória")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação é obrigatória")]
    [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não coincidem")]
    public string ConfirmarNovaSenha { get; set; } = string.Empty;
}
