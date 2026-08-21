using iskolaraktarBackend.Models;

namespace iskolaraktarBackend.Data;

/// <summary>
/// Az auth.json-t kezeli: első indításkor még nem létezik, a setup végpont hozza létre.
/// A betöltött konfiguráció a memóriában marad, minden módosítás után visszaírásra kerül a lemezre.
/// </summary>
public interface IAuthConfigService
{
    bool IsInitialized { get; }

    Task InitializeAsync(string username, string password, string databaseName, CancellationToken cancellationToken = default);

    IReadOnlyList<AuthUserInfo> GetUsers();

    Task<AuthUserInfo> CreateUserAsync(string username, string password, bool isAdmin, CancellationToken cancellationToken = default);

    Task<bool> DeleteUserAsync(string username, CancellationToken cancellationToken = default);

    Task<AuthUserInfo?> SetTableAccessAsync(string username, string tableName, AccessLevel access, CancellationToken cancellationToken = default);

    Task<AuthUserInfo?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);
}
