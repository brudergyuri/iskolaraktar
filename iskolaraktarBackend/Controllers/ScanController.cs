using iskolaraktarBackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace iskolaraktarBackend.Controllers;

[ApiController]
[Route("api/scan")]
public class ScanController : ControllerBase
{
    private readonly IDynamicTableRepository _repository;

    public ScanController(
        IDynamicTableRepository repository
    )
    {
        _repository = repository;
    }

    [HttpPost("{qrGuid:guid}")]
    public async Task<IActionResult> Scan(
        Guid qrGuid,
        CancellationToken cancellationToken
    )
    {
        var tables =
            await _repository.GetTableNamesAsync(
                cancellationToken
            );

        foreach (var tableName in tables)
        {
            var item =
                await _repository.ScanAsync(
                    tableName,
                    qrGuid.ToString(),
                    cancellationToken
                );

            if (item is not null)
            {
                return Ok(new
                {
                    tableName,
                    item
                });
            }
        }

        return NotFound(
            "Nem található leltári tétel ezzel a QR-azonosítóval."
        );
    }
}
