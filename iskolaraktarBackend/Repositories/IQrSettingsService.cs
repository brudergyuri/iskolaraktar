using iskolaraktarBackend.Models;

namespace iskolaraktarBackend.Repositories;

/// <summary>
/// A QR-kódokhoz használt publikus frontend cím
/// lekérését és módosítását biztosító szolgáltatás.
/// </summary>
public interface IQrSettingsService
{
    QrSettings GetSettings();

    Task<QrSettings> UpdateSettingsAsync(
        string baseUrl,
        CancellationToken cancellationToken = default
    );
}