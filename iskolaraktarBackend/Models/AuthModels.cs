using System.Text.Json.Serialization;

namespace iskolaraktarBackend.Models;

/// <summary>Egy felhasználó adott táblához való hozzáférési szintje.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccessLevel
{
    /// <summary>Nincs hozzáférés: a tábla adatai nem láthatók/módosíthatók.</summary>
    None,
    /// <summary>Csak olvasási jog: a tábla adatai lekérdezhetők, de nem módosíthatók.</summary>
    Read,
    /// <summary>Teljes jogkör: olvasás, írás (létrehozás/módosítás) és törlés is engedélyezett.</summary>
    ReadWrite,
}

/// <summary>Az auth.json-ban tárolt egy felhasználó, jelszóhash-sel együtt. Nem kerül ki API válaszban.</summary>
public class AuthUser
{
    public string Username { get; set; } = string.Empty;
    /// <summary>BCrypt-tel generált hash (a só a hash része), sosem sima szöveges jelszó.</summary>
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>Admin felhasználó minden táblához automatikusan teljes jogkörrel rendelkezik, a Permissions listától függetlenül.</summary>
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
    /// <summary>Az iskola/intézmény adatbázisának neve, amit az első indításos setup során ad meg a felhasználó.</summary>
    public string DatabaseName { get; set; } = string.Empty;
    public List<AuthUser> Users { get; set; } = new();
}

/// <summary>Első indításos beállítás (POST /api/auth/setup) törzse: ekkor jön létre az admin felhasználó és maga az auth.json.</summary>
public class SetupRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

/// <summary>Bejelentkezés (POST /api/auth/login) törzse.</summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Új felhasználó létrehozása (POST /api/auth/users) törzse.</summary>
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

/// <summary>Egy felhasználó tábla-hozzáférésének beállítása (PUT /api/auth/users/{username}/permissions/{tableName}) törzse.</summary>
public class SetPermissionRequest
{
    public AccessLevel Access { get; set; }
}
