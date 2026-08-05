using CarRent.Application.DTOs.VehicleCatalog;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrandsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleCatalogService _catalogService;

    public BrandsController(IVehicleCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _catalogService.GetBrandsAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] SaveBrandRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _catalogService.CreateBrandAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveBrandRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _catalogService.UpdateBrandAsync(id, request, cancellationToken));
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
            await _catalogService.DeleteBrandAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
