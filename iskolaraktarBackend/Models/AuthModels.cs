using System.Text.Json.Serialization;

namespace iskolaraktarBackend.Models;

/// <summary>Egy felhasználó adott táblához való hozzáférési szintje.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccessLevel
{
    None,
    Read,
    ReadWrite,
}

/// <summary>Az auth.json-ban tárolt egy felhasználó, jelszóhash-sel együtt. Nem kerül ki API válaszban.</summary>
public class AuthUser
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }

    /// <summary>Táblanév -> hozzáférési szint. A "*" kulcs az összes, itt fel nem sorolt táblára vonatkozó alapértelmezés.</summary>
    public Dictionary<string, AccessLevel> Permissions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>API-n kiadható, jelszóhash nélküli felhasználó-nézet.</summary>
public class AuthUserInfo
{
    public string Username { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public Dictionary<string, AccessLevel> Permissions { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public static AuthUserInfo FromUser(AuthUser user) => new()
    {
        Username = user.Username,
        IsAdmin = user.IsAdmin,
        Permissions = new Dictionary<string, AccessLevel>(user.Permissions, StringComparer.OrdinalIgnoreCase),
    };
}

/// <summary>Az auth.json fájl teljes tartalma.</summary>
public class AuthConfig
{
    public string DatabaseName { get; set; } = string.Empty;
    public List<AuthUser> Users { get; set; } = new();
}

public class SetupRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

public class SetPermissionRequest
{
    public AccessLevel Access { get; set; }
}
