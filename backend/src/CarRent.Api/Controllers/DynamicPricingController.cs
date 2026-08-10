using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/ai/pricing")]
[Authorize(Roles = AdminRoles)]
public class DynamicPricingController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IDynamicPricingService _pricingService;

    public DynamicPricingController(IDynamicPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] Guid vehicleId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _pricingService.GetSuggestedPriceAsync(vehicleId, startDate, endDate, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
