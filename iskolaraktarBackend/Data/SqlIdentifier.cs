using System.Text.RegularExpressions;

namespace iskolaraktarBackend.Data;

/// <summary>
/// Tábla- és oszlopnevek validálása, mivel ezek nem paraméterezhetők SQL-ben,
/// így közvetlen string-interpoláció előtt mindig ellenőrizni kell őket.
/// </summary>
public static partial class SqlIdentifier
{
    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]{0,63}$")]
    private static partial Regex ValidNamePattern();

    public static void EnsureValid(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name) || !ValidNamePattern().IsMatch(name))
        {
            throw new ArgumentException($"Érvénytelen azonosító: '{name}'.", paramName);
        }
    }

    public static string Quote(string name) => $"`{name.Replace("`", "``")}`";
}
