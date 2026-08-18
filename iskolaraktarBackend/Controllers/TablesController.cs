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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTables(CancellationToken cancellationToken)
    {
        return Ok(await _repository.GetTableNamesAsync(cancellationToken));
    }

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
