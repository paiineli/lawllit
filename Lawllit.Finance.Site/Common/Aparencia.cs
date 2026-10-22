using System.Security.Claims;

namespace Lawllit.Finance.Site.Common;

// O banco guarda dark, light e high-contrast. O CSS do Lawllit.Web fala escuro, claro e alto-contraste.
public static class Aparencia
{
    public static string Tema(ClaimsPrincipal usuario) => usuario.FindFirst("tema")?.Value switch
    {
        "light" => "claro",
        "high-contrast" => "alto-contraste",
        _ => "escuro",
    };

    // Nulo some com o atributo no HTML, e o tamanho normal não precisa dele.
    public static string? TamanhoFonte(ClaimsPrincipal usuario) => usuario.FindFirst("tamanho_fonte")?.Value switch
    {
        "large" => "grande",
        "xlarge" => "maior",
        _ => null,
    };
}
