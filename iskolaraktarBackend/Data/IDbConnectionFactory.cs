using MySqlConnector;

namespace iskolaraktarBackend.Data;

public interface IDbConnectionFactory
{
    Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
