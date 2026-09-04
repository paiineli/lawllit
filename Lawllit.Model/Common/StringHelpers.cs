using System.Globalization;

namespace Lawllit.Model.Common;

public static class StringHelpers
{
    public static string ToTitleCase(string value)
        => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim().ToLower());
}
