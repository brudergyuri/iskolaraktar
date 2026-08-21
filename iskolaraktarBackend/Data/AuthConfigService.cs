using System.Text.Json;
using iskolaraktarBackend.Models;

namespace iskolaraktarBackend.Data;

public class AuthConfigService : IAuthConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private AuthConfig? _config;

    public AuthConfigService(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredPath = configuration["AuthConfigPath"];
        _filePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "auth.json")
            : configuredPath;

        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _config = JsonSerializer.Deserialize<AuthConfig>(json, JsonOptions);
        }
    }

    public bool IsInitialized => _config is not null;

    public async Task InitializeAsync(string username, string password, string databaseName, CancellationToken cancellationToken = default)
    {
        ValidateUsername(username);
        ValidatePassword(password);
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Az adatbázis nevet meg kell adni.", nameof(databaseName));
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_config is not null)
            {
                throw new InvalidOperationException("A rendszer már inicializálva van.");
            }

            var adminUser = new AuthUser
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsAdmin = true,
                Permissions = new Dictionary<string, AccessLevel>(StringComparer.OrdinalIgnoreCase) { ["*"] = AccessLevel.ReadWrite },
            };

            _config = new AuthConfig
            {
                DatabaseName = databaseName,
                Users = new List<AuthUser> { adminUser },
            };

            await SaveAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public IReadOnlyList<AuthUserInfo> GetUsers()
    {
        var config = RequireConfig();
        return config.Users.Select(AuthUserInfo.FromUser).ToList();
    }

    public async Task<AuthUserInfo> CreateUserAsync(string username, string password, bool isAdmin, CancellationToken cancellationToken = default)
    {
        ValidateUsername(username);
        ValidatePassword(password);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var config = RequireConfig();
            if (config.Users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A(z) '{username}' felhasználó már létezik.");
            }

            var user = new AuthUser
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsAdmin = isAdmin,
                Permissions = new Dictionary<string, AccessLevel>(StringComparer.OrdinalIgnoreCase),
            };
            config.Users.Add(user);

            await SaveAsync(cancellationToken);
            return AuthUserInfo.FromUser(user);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteUserAsync(string username, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var config = RequireConfig();
            var user = config.Users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                return false;
            }

            config.Users.Remove(user);
            await SaveAsync(cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<AuthUserInfo?> SetTableAccessAsync(string username, string tableName, AccessLevel access, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("A tábla nevet meg kell adni.", nameof(tableName));
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var config = RequireConfig();
            var user = config.Users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                return null;
            }

            user.Permissions[tableName] = access;
            await SaveAsync(cancellationToken);
            return AuthUserInfo.FromUser(user);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<AuthUserInfo?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var config = RequireConfig();
            var user = config.Users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
            if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return AuthUserInfo.FromUser(user);
        }
        finally
        {
            _lock.Release();
        }
    }

    private AuthConfig RequireConfig() =>
        _config ?? throw new InvalidOperationException("A rendszer még nincs inicializálva.");

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(_config, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("A felhasználónevet meg kell adni.", nameof(username));
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
        {
            throw new ArgumentException("A jelszónak legalább 4 karakter hosszúnak kell lennie.", nameof(password));
        }
    }
}
