using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/vehicle-warranties")]
[Authorize(Roles = AdminRoles)]
public class VehicleWarrantiesController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleWarrantyService _warrantyService;

    public VehicleWarrantiesController(IVehicleWarrantyService warrantyService)
    {
        _warrantyService = warrantyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vehicleId, CancellationToken cancellationToken) =>
        Ok(await _warrantyService.GetAllAsync(vehicleId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleWarrantyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _warrantyService.CreateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
