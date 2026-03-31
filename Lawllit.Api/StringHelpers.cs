using System.Globalization;

namespace Lawllit.Api;

public static class StringHelpers
{
    public static string ToTitleCase(string value)
        => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim().ToLower());
}
