using CarRent.Application.DTOs.VehicleCatalog;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/vehicle-categories")]
[Authorize]
public class VehicleCategoriesController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleCatalogService _catalogService;

    public VehicleCategoriesController(IVehicleCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _catalogService.GetCategoriesAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] SaveVehicleCategoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _catalogService.CreateCategoryAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveVehicleCategoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _catalogService.UpdateCategoryAsync(id, request, cancellationToken));
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
            await _catalogService.DeleteCategoryAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
