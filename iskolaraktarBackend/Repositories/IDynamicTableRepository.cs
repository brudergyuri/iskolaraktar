using iskolaraktarBackend.Models;

namespace iskolaraktarBackend.Repositories;

/// <summary>
/// Tetszőleges, futásidőben létrehozott/bővített táblákkal dolgozó, séma-agnosztikus repository.
/// A tábla-/oszlopneveket mindig az adatbázis INFORMATION_SCHEMA-jából ellenőrzi le, mielőtt SQL-be kerülnének.
/// </summary>
public interface IDynamicTableRepository
{
    Task<IReadOnlyList<string>> GetTableNamesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ColumnInfo>> GetColumnsAsync(string tableName, CancellationToken cancellationToken = default);
    Task CreateTableAsync(TableDefinition table, CancellationToken cancellationToken = default);
    Task AddColumnAsync(string tableName, ColumnDefinition column, CancellationToken cancellationToken = default);
    Task DropTableAsync(string tableName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dictionary<string, object?>>> GetAllAsync(string tableName, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object?>?> GetByIdAsync(string tableName, object id, CancellationToken cancellationToken = default);
    Task<object> InsertAsync(string tableName, Dictionary<string, object?> values, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(string tableName, object id, Dictionary<string, object?> values, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string tableName, object id, CancellationToken cancellationToken = default);
}
