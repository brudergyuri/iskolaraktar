using MySqlConnector;

namespace iskolaraktarBackend.Data;

/// <summary>Nyitott MySQL kapcsolatok előállítására szolgáló gyár, hogy a kapcsolati logika egy helyen legyen és tesztben lecserélhető legyen.</summary>
public interface IDbConnectionFactory
{
    /// <summary>Létrehoz és megnyit egy új MySQL kapcsolatot; a hívó felelőssége a bezárása (using/await using).</summary>
    Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
