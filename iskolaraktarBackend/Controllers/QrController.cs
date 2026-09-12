using iskolaraktarBackend.Repositories;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace iskolaraktarBackend.Controllers;

[ApiController]
[Route("api/qr")]
public class QrController : ControllerBase
{
    private readonly IQrSettingsService _qrSettingsService;

    public QrController(
        IQrSettingsService qrSettingsService
    )
    {
        _qrSettingsService = qrSettingsService;
    }

    [HttpGet("{qrGuid}.png")]
    public IActionResult GetQrCode(Guid qrGuid)
    {
        var settings =
            _qrSettingsService.GetSettings();

        var qrUrl =
            $"{settings.BaseUrl.TrimEnd('/')}/scan/{qrGuid}";

        using var qrGenerator =
            new QRCodeGenerator();

        using var qrCodeData =
            qrGenerator.CreateQrCode(
                qrUrl,
                QRCodeGenerator.ECCLevel.Q
            );

        var qrCode =
            new PngByteQRCode(qrCodeData);

        var qrBytes =
            qrCode.GetGraphic(20);

        return File(
            qrBytes,
            "image/png"
        );
    }
}