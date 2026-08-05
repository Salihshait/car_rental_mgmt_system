using CarRent.Application.DTOs.VehicleCatalog;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModelsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleCatalogService _catalogService;

    public ModelsController(IVehicleCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _catalogService.GetModelsAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] SaveModelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _catalogService.CreateModelAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveModelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _catalogService.UpdateModelAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _catalogService.DeleteModelAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
