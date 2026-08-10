using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/ai/revenue-forecast")]
[Authorize(Roles = AdminRoles)]
public class RevenuePredictionController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IRevenuePredictionService _revenuePredictionService;

    public RevenuePredictionController(IRevenuePredictionService revenuePredictionService)
    {
        _revenuePredictionService = revenuePredictionService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int monthsAhead, CancellationToken cancellationToken) =>
        Ok(await _revenuePredictionService.GetForecastAsync(monthsAhead <= 0 ? 3 : monthsAhead, cancellationToken));
}
