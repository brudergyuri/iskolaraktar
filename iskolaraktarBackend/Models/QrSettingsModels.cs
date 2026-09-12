namespace iskolaraktarBackend.Models;

/// <summary>
/// A QR-kódokba kerülő publikus frontend cím beállítása.
/// Példa: https://szerver.raktar.com
/// </summary>
public class QrSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// Az admin felületről érkező QR-cím módosítási kérés.
/// </summary>
public class UpdateQrSettingsRequest
{
    public string BaseUrl { get; set; } = string.Empty;
}