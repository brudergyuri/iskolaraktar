using iskolaraktarBackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace iskolaraktarBackend.Controllers;

/// <summary>Generikus CRUD végpontok, amelyek bármelyik (futásidőben létrehozott) táblával működnek.</summary>
[ApiController]
[Route("api/data/{tableName}")]
public class DataController : ControllerBase
{
    private readonly IDynamicTableRepository _repository;

    public DataController(IDynamicTableRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string tableName, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _repository.GetAllAsync(tableName, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string tableName, int id, CancellationToken cancellationToken)
    {
        try
        {
            var row = await _repository.GetByIdAsync(tableName, id, cancellationToken);
            return row is null ? NotFound() : Ok(row);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(string tableName, [FromBody] Dictionary<string, object?> values, CancellationToken cancellationToken)
    {
        try
        {
            var newId = await _repository.InsertAsync(tableName, values, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { tableName, id = newId }, values);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string tableName, int id, [FromBody] Dictionary<string, object?> values, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _repository.UpdateAsync(tableName, id, values, cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string tableName, int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _repository.DeleteAsync(tableName, id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>QR-kód beolvasásakor hívandó: a QrGuid alapján frissíti a legutóbbi leltározás dátumát a szerver idejére.</summary>
    [HttpPost("scan/{qrGuid}")]
    public async Task<IActionResult> Scan(string tableName, string qrGuid, CancellationToken cancellationToken)
    {
        try
        {
            var row = await _repository.ScanAsync(tableName, qrGuid, cancellationToken);
            return row is null ? NotFound() : Ok(row);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
