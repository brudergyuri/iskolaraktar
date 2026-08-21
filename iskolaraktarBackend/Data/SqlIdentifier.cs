using System.Text.RegularExpressions;

namespace iskolaraktarBackend.Data;

/// <summary>
/// Tábla- és oszlopnevek validálása, mivel ezek nem paraméterezhetők SQL-ben,
/// így közvetlen string-interpoláció előtt mindig ellenőrizni kell őket.
/// </summary>
public static partial class SqlIdentifier
{
    /// <summary>Betűvel/aláhúzással kezdődő, csak alfanumerikus és aláhúzás karaktereket tartalmazó, max. 64 hosszú név.</summary>
    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]{0,63}$")]
    private static partial Regex ValidNamePattern();

    /// <summary>Eldobja a kivételt, ha a név nem felel meg a biztonságos SQL azonosító mintának (pl. SQL injection elleni védelem).</summary>
    public static void EnsureValid(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name) || !ValidNamePattern().IsMatch(name))
        {
            throw new ArgumentException($"Érvénytelen azonosító: '{name}'.", paramName);
        }
    }

    /// <summary>Backtick-be teszi a nevet (MySQL azonosító-idézés), a névben lévő backtice-eket megduplázva.</summary>
    public static string Quote(string name) => $"`{name.Replace("`", "``")}`";
}
