using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class AnalyzeDamageRequest
{
    public Guid? VehicleId { get; set; }
    public Guid? RentalId { get; set; }
    public IFormFile? Image { get; set; }
}

[ApiController]
[Route("api/ai/damage-detection")]
[Authorize(Roles = AdminRoles)]
public class DamageDetectionController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IDamageDetectionService _damageDetectionService;

    public DamageDetectionController(IDamageDetectionService damageDetectionService)
    {
        _damageDetectionService = damageDetectionService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromForm] AnalyzeDamageRequest request, CancellationToken cancellationToken)
    {
        if (request.Image is null || request.Image.Length == 0)
        {
            return BadRequest(new { message = "An image file is required." });
        }

        using var stream = new MemoryStream();
        await request.Image.CopyToAsync(stream, cancellationToken);

        var result = await _damageDetectionService.AnalyzeAsync(request.VehicleId, request.RentalId, request.Image.FileName, stream.ToArray(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid? vehicleId, CancellationToken cancellationToken) =>
        Ok(await _damageDetectionService.GetHistoryAsync(vehicleId, cancellationToken));
}
