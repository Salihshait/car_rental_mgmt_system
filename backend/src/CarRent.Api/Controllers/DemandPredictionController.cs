using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/ai/demand")]
[Authorize(Roles = AdminRoles)]
public class DemandPredictionController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IDemandPredictionService _demandPredictionService;

    public DemandPredictionController(IDemandPredictionService demandPredictionService)
    {
        _demandPredictionService = demandPredictionService;
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast([FromQuery] Guid? branchId, [FromQuery] Guid? categoryId, [FromQuery] int monthsAhead, CancellationToken cancellationToken) =>
        Ok(await _demandPredictionService.GetForecastAsync(branchId, categoryId, monthsAhead <= 0 ? 3 : monthsAhead, cancellationToken));
}
