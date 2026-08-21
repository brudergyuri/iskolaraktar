namespace iskolaraktarBackend.Models;

/// <summary>Egy új tábla létrehozásakor/bővítésekor megadott egy oszlop leírása.</summary>
public class ColumnDefinition
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Pl. "VARCHAR(255)", "INT", "DECIMAL(10,2)".</summary>
    public string SqlType { get; set; } = string.Empty;

    public bool IsNullable { get; set; } = true;
}

/// <summary>Egy új dinamikus tábla létrehozásához szükséges adatok (név + kliens által megadott oszlopok, a fix oszlopokon felül).</summary>
public class TableDefinition
{
    public string Name { get; set; } = string.Empty;
    public List<ColumnDefinition> Columns { get; set; } = new();
}

/// <summary>Egy már létező tábla egy oszlopának az INFORMATION_SCHEMA-ból kiolvasott leírása.</summary>
public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
}
