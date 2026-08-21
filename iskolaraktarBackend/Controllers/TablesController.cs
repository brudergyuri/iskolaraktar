using iskolaraktarBackend.Models;
using iskolaraktarBackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace iskolaraktarBackend.Controllers;

/// <summary>Táblák és oszlopok futásidejű létrehozása/bővítése (admin funkció).</summary>
[ApiController]
[Route("api/tables")]
public class TablesController : ControllerBase
{
    private readonly IDynamicTableRepository _repository;

    public TablesController(IDynamicTableRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Az adatbázisban lévő összes tábla nevének listája.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTables(CancellationToken cancellationToken)
    {
        return Ok(await _repository.GetTableNamesAsync(cancellationToken));
    }

    /// <summary>Egy adott tábla oszlopainak leírása (név, típus, nullable, primáry key).</summary>
    [HttpGet("{tableName}/columns")]
    public async Task<ActionResult<IReadOnlyList<ColumnInfo>>> GetColumns(string tableName, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _repository.GetColumnsAsync(tableName, cancellationToken));
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

    /// <summary>Új dinamikus tábla létrehozása; a fix (Id/AssetCode/QrGuid/LastInventoryDate) oszlopok automatikusan bekerülnek.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateTable(TableDefinition table, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.CreateTableAsync(table, cancellationToken);
            return CreatedAtAction(nameof(GetColumns), new { tableName = table.Name }, null);
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

    /// <summary>Új oszlop hozzáadása egy már létező táblához (ALTER TABLE ... ADD COLUMN).</summary>
    [HttpPost("{tableName}/columns")]
    public async Task<IActionResult> AddColumn(string tableName, ColumnDefinition column, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.AddColumnAsync(tableName, column, cancellationToken);
            return NoContent();
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

    /// <summary>Teljes tábla törlése (DROP TABLE), minden benne lévő adattal együtt.</summary>
    [HttpDelete("{tableName}")]
    public async Task<IActionResult> DropTable(string tableName, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.DropTableAsync(tableName, cancellationToken);
            return NoContent();
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
