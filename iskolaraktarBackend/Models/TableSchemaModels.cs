namespace iskolaraktarBackend.Models;

public class ColumnDefinition
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Pl. "VARCHAR(255)", "INT", "DECIMAL(10,2)".</summary>
    public string SqlType { get; set; } = string.Empty;

    public bool IsNullable { get; set; } = true;
}

public class TableDefinition
{
    public string Name { get; set; } = string.Empty;
    public List<ColumnDefinition> Columns { get; set; } = new();
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
}
