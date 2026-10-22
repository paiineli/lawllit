using System.Net.Mail;

namespace Lawllit.Model.Common;

// Os contratos repetem as regras dos DataAnnotations dos sites para a API não depender deles.
public static class Validacao
{
    public static bool Tamanho(string? valor, int minimo, int maximo)
    {
        var tamanho = valor?.Trim().Length ?? 0;
        return tamanho >= minimo && tamanho <= maximo;
    }

    public static bool Email(string? valor)
        => !string.IsNullOrWhiteSpace(valor)
            && valor.Length <= 255
            && MailAddress.TryCreate(valor.Trim(), out var endereco)
            && endereco.Address == valor.Trim();

    public const int SenhaMinima = 6;
}
