using System.Text.Json;
using iskolaraktarBackend.Models;

namespace iskolaraktarBackend.Repositories;

/// <summary>
/// A QR-kódokhoz használt publikus frontend cím
/// beolvasását és tartós módosítását végzi a qrsettings.json fájlban.
/// </summary>
public class QrSettingsService : IQrSettingsService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public QrSettingsService(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(
            environment.ContentRootPath,
            "qrsettings.json"
        );
    }

    public QrSettings GetSettings()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                "A qrsettings.json fájl nem található.",
                _filePath
            );
        }

        var json = File.ReadAllText(_filePath);

        var config = JsonSerializer.Deserialize<QrSettingsFile>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        return config?.QrSettings
            ?? throw new InvalidOperationException(
                "A qrsettings.json fájl tartalma érvénytelen."
            );
    }

    public async Task<QrSettings> UpdateSettingsAsync(
        string baseUrl,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException(
                "A QR szervercím nem lehet üres.",
                nameof(baseUrl)
            );
        }

        var normalizedBaseUrl =
            NormalizeBaseUrl(baseUrl);

        var settings = new QrSettings
        {
            BaseUrl = normalizedBaseUrl
        };

        var config = new QrSettingsFile
        {
            QrSettings = settings
        };

        var json = JsonSerializer.Serialize(
            config,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }
        );

        await _lock.WaitAsync(cancellationToken);

        try
        {
            await File.WriteAllTextAsync(
                _filePath,
                json,
                cancellationToken
            );
        }
        finally
        {
            _lock.Release();
        }

        return settings;
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var value = baseUrl.Trim();

        if (!value.StartsWith(
                "http://",
                StringComparison.OrdinalIgnoreCase
            ) &&
            !value.StartsWith(
                "https://",
                StringComparison.OrdinalIgnoreCase
            ))
        {
            value = $"https://{value}";
        }

        if (!Uri.TryCreate(
                value,
                UriKind.Absolute,
                out var uri
            ) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "Érvényes HTTP vagy HTTPS címet adj meg.",
                nameof(baseUrl)
            );
        }

        return value.TrimEnd('/');
    }

    private class QrSettingsFile
    {
        public QrSettings QrSettings { get; set; } = new();
    }
}