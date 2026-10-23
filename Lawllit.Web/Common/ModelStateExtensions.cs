using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lawllit.Web.Common;

public static class ModelStateExtensions
{
    // A tela mostra uma mensagem só, a primeira com texto, e a reserva quando a validação não deu nenhuma.
    public static string PrimeiroErro(this ModelStateDictionary estado, string reserva)
        => estado.Values
            .SelectMany(entrada => entrada.Errors)
            .Select(erro => erro.ErrorMessage)
            .FirstOrDefault(mensagem => !string.IsNullOrWhiteSpace(mensagem))
            ?? reserva;
}
