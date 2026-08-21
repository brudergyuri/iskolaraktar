using iskolaraktarBackend.Models;

namespace iskolaraktarBackend.Repositories;

/// <summary>
/// Tetszőleges, futásidőben létrehozott/bővített táblákkal dolgozó, séma-agnosztikus repository.
/// A tábla-/oszlopneveket mindig az adatbázis INFORMATION_SCHEMA-jából ellenőrzi le, mielőtt SQL-be kerülnének.
/// </summary>
public interface IDynamicTableRepository
{
    /// <summary>Az adatbázisban lévő összes tábla nevét adja vissza (INFORMATION_SCHEMA.TABLES alapján).</summary>
    Task<IReadOnlyList<string>> GetTableNamesAsync(CancellationToken cancellationToken = default);
    /// <summary>Egy tábla oszlopainak leírása (név, típus, nullable, primáry key).</summary>
    Task<IReadOnlyList<ColumnInfo>> GetColumnsAsync(string tableName, CancellationToken cancellationToken = default);
    /// <summary>Új tábla létrehozása a fix (Id/AssetCode/QrGuid/LastInventoryDate) és a megadott extra oszlopokkal.</summary>
    Task CreateTableAsync(TableDefinition table, CancellationToken cancellationToken = default);
    /// <summary>Új oszlop hozzáadása egy már létező táblához (ALTER TABLE).</summary>
    Task AddColumnAsync(string tableName, ColumnDefinition column, CancellationToken cancellationToken = default);
    /// <summary>Teljes tábla törlése (DROP TABLE), minden adatával együtt.</summary>
    Task DropTableAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>Egy tábla összes sorát adja vissza, oszlopnév -> érték párok listájaként.</summary>
    Task<IReadOnlyList<Dictionary<string, object?>>> GetAllAsync(string tableName, CancellationToken cancellationToken = default);
    /// <summary>Egy sort ad vissza a primáry key alapján, vagy null-t, ha nincs ilyen.</summary>
    Task<Dictionary<string, object?>?> GetByIdAsync(string tableName, object id, CancellationToken cancellationToken = default);
    /// <summary>Új sor beszúrása; a QrGuid-ot mindig a szerver generálja, a beérkezett érték figyelmen kívül marad.</summary>
    Task<object> InsertAsync(string tableName, Dictionary<string, object?> values, CancellationToken cancellationToken = default);
    /// <summary>Egy meglévő sor mezőinek frissítése; a QrGuid itt sem módosítható.</summary>
    Task<bool> UpdateAsync(string tableName, object id, Dictionary<string, object?> values, CancellationToken cancellationToken = default);
    /// <summary>Egy sor törlése a primáry key alapján.</summary>
    Task<bool> DeleteAsync(string tableName, object id, CancellationToken cancellationToken = default);

    /// <summary>QR-kód beolvasáskor a QrGuid alapján beazonosítja az eszközt, és a szerver idejére frissíti a legutóbbi leltározás dátumát.</summary>
    Task<Dictionary<string, object?>?> ScanAsync(string tableName, string qrGuid, CancellationToken cancellationToken = default);
}
