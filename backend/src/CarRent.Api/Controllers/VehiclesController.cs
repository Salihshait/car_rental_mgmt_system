using System.Security.Claims;
using CarRent.Application.DTOs.Vehicles;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    private Guid? CurrentUserId
    {
        get
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? brandId,
        [FromQuery] Guid? branchId,
        [FromQuery] string? fuelType,
        [FromQuery] string? transmission,
        CancellationToken cancellationToken)
    {
        var filter = new VehicleFilter
        {
            Search = search,
            Status = status,
            CategoryId = categoryId,
            BrandId = brandId,
            BranchId = branchId,
            FuelType = fuelType,
            Transmission = transmission
        };

        return Ok(await _vehicleService.GetAllAsync(filter, cancellationToken));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await _vehicleService.GetDashboardAsync(cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] Guid? brandId,
        [FromQuery] Guid? branchId,
        CancellationToken cancellationToken)
    {
        var filter = new VehicleFilter { Search = search, Status = status, BrandId = brandId, BranchId = branchId };
        var csv = await _vehicleService.ExportAsync(filter, cancellationToken);
        return File(csv, "text/csv", "vehicles.csv");
    }

    [HttpPost("import")]
    [Authorize(Roles = AdminRoles)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        using var stream = file.OpenReadStream();
        var result = await _vehicleService.ImportAsync(stream, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id, cancellationToken);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpGet("{id:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken cancellationToken) =>
        Ok(await _vehicleService.GetTimelineAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] SaveVehicleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _vehicleService.CreateAsync(request, CurrentUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveVehicleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _vehicleService.UpdateAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateVehicleStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _vehicleService.UpdateStatusAsync(id, request.Status, CurrentUserId, cancellationToken));
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
        var deleted = await _vehicleService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
