using System.Security.Claims;
using CarRent.Application.DTOs.Drivers;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/driver-ratings")]
[Authorize(Roles = AdminRoles)]
public class DriverRatingsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IDriverRatingService _ratingService;

    public DriverRatingsController(IDriverRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? driverId, CancellationToken cancellationToken) =>
        Ok(await _ratingService.GetAllAsync(driverId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateDriverRatingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _ratingService.AddAsync(request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
