using MySqlConnector;

namespace iskolaraktarBackend.Data;

public class StartupInitializer
{
    private readonly IConfiguration _configuration;
    private readonly IAuthConfigService _authConfigService;

    public StartupInitializer(
        IConfiguration configuration,
        IAuthConfigService authConfigService)
    {
        _configuration = configuration;
        _authConfigService = authConfigService;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            _configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Missing 'ConnectionStrings:Default' configuration value."
            );

        var builder =
            new MySqlConnectionStringBuilder(connectionString);

        var databaseName = builder.Database;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException(
                "Az adatbázis neve nincs megadva a dbsettings.json fájlban."
            );
        }

        // Először adatbázis megadása nélkül csatlakozunk a MySQL szerverhez.
        builder.Database = "";

        await using var connection =
            new MySqlConnection(builder.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        var safeDatabaseName =
            databaseName.Replace("`", "``");

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            $"CREATE DATABASE IF NOT EXISTS `{safeDatabaseName}` " +
            "CHARACTER SET utf8mb4;";

        await command.ExecuteNonQueryAsync(
            cancellationToken
        );

        // Ha még nincs auth.json, automatikusan létrehozzuk
        // a bemutatóhoz használható első admin felhasználót.
        if (!_authConfigService.IsInitialized)
        {
            await _authConfigService.InitializeAsync(
                "admin",
                "admin123",
                databaseName,
                cancellationToken
            );
        }
    }
}