using iskolaraktarBackend.Models;

namespace iskolaraktarBackend.Data;

/// <summary>
/// Az auth.json-t kezeli: első indításkor még nem létezik, a setup végpont hozza létre.
/// A betöltött konfiguráció a memóriában marad, minden módosítás után visszaírásra kerül a lemezre.
/// </summary>
public interface IAuthConfigService
{
    /// <summary>Igaz, ha az auth.json már létezik és be van töltve (azaz az első indításos beállítás megtörtént).</summary>
    bool IsInitialized { get; }

    /// <summary>Első indításkor hívandó egyszer: létrehozza az admin felhasználót (teljes jogkörrel minden táblára) és kiírja az auth.json-t.</summary>
    Task InitializeAsync(string username, string password, string databaseName, CancellationToken cancellationToken = default);

    /// <summary>Az összes felhasználó jelszóhash nélküli, biztonságosan kiadható listája.</summary>
    IReadOnlyList<AuthUserInfo> GetUsers();

    /// <summary>Új felhasználót vesz fel (kezdetben jogosultság nélkül, ha nem admin), a jelszót BCrypt-tel hash-elve tárolja.</summary>
    Task<AuthUserInfo> CreateUserAsync(string username, string password, bool isAdmin, CancellationToken cancellationToken = default);

    /// <summary>Törli a felhasználót; hamis, ha nem létezett ilyen nevű felhasználó.</summary>
    Task<bool> DeleteUserAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>Beállítja/felülírja egy felhasználó adott táblához tartozó hozzáférési szintjét (None/Read/ReadWrite).</summary>
    Task<AuthUserInfo?> SetTableAccessAsync(string username, string tableName, AccessLevel access, CancellationToken cancellationToken = default);

    /// <summary>Bejelentkezéskor hívandó: ellenőrzi a felhasználónév+jelszó párost, sikertelen esetén null-t ad vissza.</summary>
    Task<AuthUserInfo?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);
}
