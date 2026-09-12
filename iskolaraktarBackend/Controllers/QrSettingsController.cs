using iskolaraktarBackend.Models;
using iskolaraktarBackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace iskolaraktarBackend.Controllers;

/// <summary>
/// A QR-kódokhoz használt publikus frontend cím lekérése és módosítása.
/// </summary>
[ApiController]
[Route("api/qr-settings")]
public class QrSettingsController : ControllerBase
{
    private readonly IQrSettingsService _qrSettingsService;

    public QrSettingsController(
        IQrSettingsService qrSettingsService
    )
    {
        _qrSettingsService = qrSettingsService;
    }

    /// <summary>
    /// A jelenlegi QR alapcím lekérése.
    /// </summary>
    [HttpGet]
    public ActionResult<QrSettings> GetSettings()
    {
        try
        {
            return Ok(
                _qrSettingsService.GetSettings()
            );
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// A QR alapcím módosítása.
    /// Példa: https://szerver.raktar.com
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<QrSettings>> UpdateSettings(
        UpdateQrSettingsRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var settings =
                await _qrSettingsService.UpdateSettingsAsync(
                    request.BaseUrl,
                    cancellationToken
                );

            return Ok(settings);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}