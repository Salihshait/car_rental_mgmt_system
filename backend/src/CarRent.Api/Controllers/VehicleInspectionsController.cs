using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/vehicle-inspections")]
[Authorize(Roles = AdminRoles)]
public class VehicleInspectionsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleInspectionService _inspectionService;

    public VehicleInspectionsController(IVehicleInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vehicleId, CancellationToken cancellationToken) =>
        Ok(await _inspectionService.GetAllAsync(vehicleId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleInspectionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _inspectionService.CreateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
