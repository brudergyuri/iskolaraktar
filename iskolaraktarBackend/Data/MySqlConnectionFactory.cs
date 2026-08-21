using MySqlConnector;

namespace iskolaraktarBackend.Data;

/// <summary>Az <see cref="IDbConnectionFactory"/> MySQL-specifikus megvalósítása, a kapcsolati stringet a dbsettings.json-ból olvassa.</summary>
public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        // A 'ConnectionStrings:Default' kulcsot a dbsettings.json biztosítja (lásd Program.cs)
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing 'ConnectionStrings:Default' configuration value.");
    }

    /// <summary>Minden hívás egy vadonatúj, már megnyitott kapcsolatot ad vissza (nincs kapcsolat-pool kezelés a kódban, a driver intézi).</summary>
    public async Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
