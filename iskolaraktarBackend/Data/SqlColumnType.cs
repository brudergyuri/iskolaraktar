using System.Text.RegularExpressions;

namespace iskolaraktarBackend.Data;

/// <summary>
/// Csak egy zárt listából engedélyezett SQL típusokat fogad el, mivel a típus
/// is közvetlenül kerül a DDL parancsokba (nem paraméterezhető).
/// </summary>
public static partial class SqlColumnType
{
    [GeneratedRegex(
        @"^(INT|BIGINT|SMALLINT|TINYINT|BOOLEAN|BOOL|TEXT|DATE|DATETIME|TIMESTAMP|DOUBLE|FLOAT|VARCHAR\(\d{1,4}\)|CHAR\(\d{1,4}\)|DECIMAL\(\d{1,2},\d{1,2}\))$",
        RegexOptions.IgnoreCase)]
    private static partial Regex ValidTypePattern();

    public static void EnsureValid(string sqlType)
    {
        if (string.IsNullOrWhiteSpace(sqlType) || !ValidTypePattern().IsMatch(sqlType.Trim()))
        {
            throw new ArgumentException($"Nem támogatott SQL típus: '{sqlType}'.", nameof(sqlType));
        }
    }
}
