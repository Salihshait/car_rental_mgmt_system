using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/ai/maintenance-predictions")]
[Authorize(Roles = AdminRoles)]
public class PredictiveMaintenanceController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IPredictiveMaintenanceService _predictiveMaintenanceService;

    public PredictiveMaintenanceController(IPredictiveMaintenanceService predictiveMaintenanceService)
    {
        _predictiveMaintenanceService = predictiveMaintenanceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vehicleId, [FromQuery] string? status, CancellationToken cancellationToken) =>
        Ok(await _predictiveMaintenanceService.GetPredictionsAsync(vehicleId, status, cancellationToken));

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(CancellationToken cancellationToken)
    {
        await _predictiveMaintenanceService.GeneratePredictionsAsync(cancellationToken);
        return Ok(await _predictiveMaintenanceService.GetPredictionsAsync(null, "Open", cancellationToken));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePredictionStatusRequest request, CancellationToken cancellationToken) =>
        Ok(await _predictiveMaintenanceService.UpdateStatusAsync(id, request, cancellationToken));
}
