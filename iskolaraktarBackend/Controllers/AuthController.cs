using iskolaraktarBackend.Data;
using iskolaraktarBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace iskolaraktarBackend.Controllers;

/// <summary>Első indítási beállítás, bejelentkezés és felhasználó-/jogosultságkezelés.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthConfigService _authConfigService;

    public AuthController(IAuthConfigService authConfigService)
    {
        _authConfigService = authConfigService;
    }

    /// <summary>A frontend ez alapján dönti el, hogy meg kell-e jeleníteni az első indításos beállító képernyőt.</summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { initialized = _authConfigService.IsInitialized });
    }

    [HttpPost("setup")]
    public async Task<IActionResult> Setup(SetupRequest request, CancellationToken cancellationToken)
    {
        if (_authConfigService.IsInitialized)
        {
            return Conflict("A rendszer már inicializálva van.");
        }

        try
        {
            await _authConfigService.InitializeAsync(request.Username, request.Password, request.DatabaseName, cancellationToken);
            return CreatedAtAction(nameof(GetStatus), null, new { initialized = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (!_authConfigService.IsInitialized)
        {
            return BadRequest("A rendszer még nincs inicializálva.");
        }

        var user = await _authConfigService.ValidateCredentialsAsync(request.Username, request.Password, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        if (!_authConfigService.IsInitialized)
        {
            return BadRequest("A rendszer még nincs inicializálva.");
        }

        return Ok(_authConfigService.GetUsers());
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!_authConfigService.IsInitialized)
        {
            return BadRequest("A rendszer még nincs inicializálva.");
        }

        try
        {
            var user = await _authConfigService.CreateUserAsync(request.Username, request.Password, request.IsAdmin, cancellationToken);
            return CreatedAtAction(nameof(GetUsers), null, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("users/{username}")]
    public async Task<IActionResult> DeleteUser(string username, CancellationToken cancellationToken)
    {
        if (!_authConfigService.IsInitialized)
        {
            return BadRequest("A rendszer még nincs inicializálva.");
        }

        var deleted = await _authConfigService.DeleteUserAsync(username, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Egy felhasználó adott táblához való hozzáférését állítja be: None, Read vagy ReadWrite.</summary>
    [HttpPut("users/{username}/permissions/{tableName}")]
    public async Task<IActionResult> SetTableAccess(string username, string tableName, SetPermissionRequest request, CancellationToken cancellationToken)
    {
        if (!_authConfigService.IsInitialized)
        {
            return BadRequest("A rendszer még nincs inicializálva.");
        }

        try
        {
            var user = await _authConfigService.SetTableAccessAsync(username, tableName, request.Access, cancellationToken);
            return user is null ? NotFound() : Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
