namespace Lawllit.Site.Common;

// Selo com a sigla no lugar do escudo quando não existe fonte aberta para ele, que é o caso de
// LoL e Valorant e de time de Dota que o OpenDota não conhece.
public static class Escudo
{
    private const int QuantidadeCores = 8;

    private static readonly string[] PalavrasGenericas = ["team", "esports", "gaming", "club"];

    // "Paper Rex" vira PR, "G2 Esports" vira G2, e nome de uma palavra só fica inteiro se for
    // curto como LOUD, ou nas três primeiras letras, que costuma ser a sigla oficial, SEN de Sentinels.
    public static string Sigla(string nome)
    {
        var palavras = nome.Split([' ', '.', '-', '_'], StringSplitOptions.RemoveEmptyEntries);
        var relevantes = palavras.Where(palavra => !PalavrasGenericas.Contains(palavra.ToLowerInvariant())).ToArray();

        if (relevantes.Length == 0) relevantes = palavras;
        if (relevantes.Length == 0) return "?";

        if (relevantes.Length > 1)
            return string.Concat(relevantes.Take(3).Select(palavra => char.ToUpperInvariant(palavra[0])));

        var unica = relevantes[0].ToUpperInvariant();

        return unica.Length <= 4 ? unica : unica[..3];
    }

    // As cores são classes fixas do CSS, porque a CSP não deixa style inline pintar cada time. A
    // soma das letras, e não o GetHashCode, garante a mesma cor para o time em toda visita.
    public static string Classe(string nome)
        => $"escudo-cor-{nome.Sum(letra => letra) % QuantidadeCores}";
}
