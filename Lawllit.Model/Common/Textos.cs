using System.Globalization;

namespace Lawllit.Model.Common;

public static class Textos
{
    public static string ParaTitulo(string valor)
        => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valor.Trim().ToLower());
}
