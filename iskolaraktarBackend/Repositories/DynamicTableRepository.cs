using System.Text;
using System.Text.Json;
using iskolaraktarBackend.Data;
using iskolaraktarBackend.Models;
using MySqlConnector;

namespace iskolaraktarBackend.Repositories;

public class DynamicTableRepository : IDynamicTableRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DynamicTableRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<string>> GetTableNamesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() ORDER BY TABLE_NAME;",
            connection);

        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    public async Task<IReadOnlyList<ColumnInfo>> GetColumnsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await GetColumnsAsync(connection, tableName, cancellationToken);
    }

    public async Task CreateTableAsync(TableDefinition table, CancellationToken cancellationToken = default)
    {
        SqlIdentifier.EnsureValid(table.Name, nameof(table.Name));
        if (table.Columns.Count == 0)
        {
            throw new ArgumentException("Legalább egy oszlopot meg kell adni.", nameof(table));
        }

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        if (await TableExistsAsync(connection, table.Name, cancellationToken))
        {
            throw new InvalidOperationException($"A(z) '{table.Name}' tábla már létezik.");
        }

        var columnClauses = new List<string> { "`Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY" };
        foreach (var column in table.Columns)
        {
            SqlIdentifier.EnsureValid(column.Name, nameof(column.Name));
            if (string.Equals(column.Name, "Id", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Az 'Id' oszlopnév foglalt, automatikusan létrejön.", nameof(table));
            }
            SqlColumnType.EnsureValid(column.SqlType);

            var nullability = column.IsNullable ? "NULL" : "NOT NULL";
            columnClauses.Add($"{SqlIdentifier.Quote(column.Name)} {column.SqlType.Trim().ToUpperInvariant()} {nullability}");
        }

        var sql = $"CREATE TABLE {SqlIdentifier.Quote(table.Name)} ({string.Join(", ", columnClauses)});";
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddColumnAsync(string tableName, ColumnDefinition column, CancellationToken cancellationToken = default)
    {
        SqlIdentifier.EnsureValid(tableName, nameof(tableName));
        SqlIdentifier.EnsureValid(column.Name, nameof(column.Name));
        SqlColumnType.EnsureValid(column.SqlType);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableExistsAsync(connection, tableName, cancellationToken);

        var existingColumns = await GetColumnsAsync(connection, tableName, cancellationToken);
        if (existingColumns.Any(c => string.Equals(c.Name, column.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A(z) '{column.Name}' oszlop már létezik a(z) '{tableName}' táblában.");
        }

        var nullability = column.IsNullable ? "NULL" : "NOT NULL";
        var sql = $"ALTER TABLE {SqlIdentifier.Quote(tableName)} ADD COLUMN {SqlIdentifier.Quote(column.Name)} {column.SqlType.Trim().ToUpperInvariant()} {nullability};";
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DropTableAsync(string tableName, CancellationToken cancellationToken = default)
    {
        SqlIdentifier.EnsureValid(tableName, nameof(tableName));

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableExistsAsync(connection, tableName, cancellationToken);

        var sql = $"DROP TABLE {SqlIdentifier.Quote(tableName)};";
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> GetAllAsync(string tableName, CancellationToken cancellationToken = default)
    {
        SqlIdentifier.EnsureValid(tableName, nameof(tableName));

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableExistsAsync(connection, tableName, cancellationToken);

        var sql = $"SELECT * FROM {SqlIdentifier.Quote(tableName)};";
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(ReadRow(reader));
        }

        return rows;
    }

    public async Task<Dictionary<string, object?>?> GetByIdAsync(string tableName, object id, CancellationToken cancellationToken = default)
    {
        SqlIdentifier.EnsureValid(tableName, nameof(tableName));

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var primaryKey = await GetPrimaryKeyColumnAsync(connection, tableName, cancellationToken);

        var sql = $"SELECT * FROM {SqlIdentifier.Quote(tableName)} WHERE {SqlIdentifier.Quote(primaryKey)} = @Id;";
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadRow(reader) : null;
    }

    public async Task<object> InsertAsync(string tableName, Dictionary<string, object?> values, CancellationToken cancellationToken = default)
    {
        SqlIdentifier.EnsureValid(tableName, nameof(tableName));
        if (values.Count == 0)
        {
            throw new ArgumentException("Legalább egy mezőt meg kell adni.", nameof(values));
        }

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var validColumns = await GetColumnsAsync(connection, tableName, cancellationToken);
        var columnNames = ValidateAndFilterColumns(values.Keys, validColumns);

        var columnList = string.Join(", ", columnNames.Select(SqlIdentifier.Quote));
        var paramList = string.Join(", ", columnNames.Select(c => "@" + c));
        var sql = $"INSERT INTO {SqlIdentifier.Quote(tableName)} ({columnList}) VALUES ({paramList}); SELECT LAST_INSERT_ID();";

        await using var command = new MySqlCommand(sql, connection);
        foreach (var columnName in columnNames)
        {
            command.Parameters.AddWithValue("@" + columnName, ConvertValue(values[columnName]));
        }

        var newId = await command.ExecuteScalarAsync(cancellationToken);
        return newId ?? throw new InvalidOperationException("Nem sikerült beszúrni a rekordot.");
    }

    public async Task<bool> UpdateAsync(string tableName, object id, Dictionary<string, object?> values, CancellationToken cancellationToken = default)
    {
        SqlIdentifier.EnsureValid(tableName, nameof(tableName));
        if (values.Count == 0)
        {
            throw new ArgumentException("Legalább egy mezőt meg kell adni.", nameof(values));
        }

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var validColumns = await GetColumnsAsync(connection, tableName, cancellationToken);
        var columnNames = ValidateAndFilterColumns(values.Keys, validColumns);
        var primaryKey = validColumns.First(c => c.IsPrimaryKey).Name;

        var setClause = string.Join(", ", columnNames.Select(c => $"{SqlIdentifier.Quote(c)} = @{c}"));
        var sql = $"UPDATE {SqlIdentifier.Quote(tableName)} SET {setClause} WHERE {SqlIdentifier.Quote(primaryKey)} = @Id;";

        await using var command = new MySqlCommand(sql, connection);
        foreach (var columnName in columnNames)
        {
            command.Parameters.AddWithValue("@" + columnName, ConvertValue(values[columnName]));
        }
        command.Parameters.AddWithValue("@Id", id);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(string tableName, object id, CancellationToken cancellationToken = default)
    {
        SqlIdentifier.EnsureValid(tableName, nameof(tableName));

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var primaryKey = await GetPrimaryKeyColumnAsync(connection, tableName, cancellationToken);

        var sql = $"DELETE FROM {SqlIdentifier.Quote(tableName)} WHERE {SqlIdentifier.Quote(primaryKey)} = @Id;";
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    private static List<string> ValidateAndFilterColumns(IEnumerable<string> requestedColumns, IReadOnlyList<ColumnInfo> validColumns)
    {
        var validNames = new HashSet<string>(validColumns.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        foreach (var name in requestedColumns)
        {
            if (!validNames.Contains(name))
            {
                throw new ArgumentException($"Ismeretlen oszlop: '{name}'.");
            }
            result.Add(name);
        }

        return result;
    }

    private static object? ConvertValue(object? value) => value switch
    {
        null => DBNull.Value,
        JsonElement { ValueKind: JsonValueKind.Null } => DBNull.Value,
        JsonElement { ValueKind: JsonValueKind.String } je => je.GetString(),
        JsonElement { ValueKind: JsonValueKind.Number } je => je.TryGetInt64(out var l) ? l : je.GetDouble(),
        JsonElement { ValueKind: JsonValueKind.True or JsonValueKind.False } je => je.GetBoolean(),
        JsonElement je => je.GetRawText(),
        _ => value,
    };

    private static Dictionary<string, object?> ReadRow(MySqlDataReader reader)
    {
        var row = new Dictionary<string, object?>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }

        return row;
    }

    private static async Task<bool> TableExistsAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @TableName;",
            connection);
        command.Parameters.AddWithValue("@TableName", tableName);
        var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return count > 0;
    }

    private static async Task EnsureTableExistsAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(connection, tableName, cancellationToken))
        {
            throw new KeyNotFoundException($"A(z) '{tableName}' tábla nem létezik.");
        }
    }

    private static async Task<IReadOnlyList<ColumnInfo>> GetColumnsAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        SqlIdentifier.EnsureValid(tableName, nameof(tableName));
        await EnsureTableExistsAsync(connection, tableName, cancellationToken);

        await using var command = new MySqlCommand(
            """
            SELECT c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE,
                   IF(k.COLUMN_NAME IS NOT NULL, 1, 0) AS IS_PRIMARY_KEY
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
                   ON k.TABLE_SCHEMA = c.TABLE_SCHEMA
                  AND k.TABLE_NAME = c.TABLE_NAME
                  AND k.COLUMN_NAME = c.COLUMN_NAME
                  AND k.CONSTRAINT_NAME = 'PRIMARY'
            WHERE c.TABLE_SCHEMA = DATABASE() AND c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION;
            """,
            connection);
        command.Parameters.AddWithValue("@TableName", tableName);

        var columns = new List<ColumnInfo>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(new ColumnInfo
            {
                Name = reader.GetString(0),
                DataType = reader.GetString(1),
                IsNullable = reader.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                IsPrimaryKey = reader.GetInt32(3) == 1,
            });
        }

        return columns;
    }

    private static async Task<string> GetPrimaryKeyColumnAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        var columns = await GetColumnsAsync(connection, tableName, cancellationToken);
        var primaryKey = columns.FirstOrDefault(c => c.IsPrimaryKey)
            ?? throw new InvalidOperationException($"A(z) '{tableName}' táblának nincs elsődleges kulcsa.");
        return primaryKey.Name;
    }
}
