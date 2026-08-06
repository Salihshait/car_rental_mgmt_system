using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/spare-parts")]
[Authorize(Roles = AdminRoles)]
public class SparePartsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly ISparePartService _sparePartService;

    public SparePartsController(ISparePartService sparePartService)
    {
        _sparePartService = sparePartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _sparePartService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var part = await _sparePartService.GetByIdAsync(id, cancellationToken);
        return part is null ? NotFound() : Ok(part);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveSparePartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _sparePartService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveSparePartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _sparePartService.UpdateAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/adjust-stock")]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] AdjustStockRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _sparePartService.AdjustStockAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
