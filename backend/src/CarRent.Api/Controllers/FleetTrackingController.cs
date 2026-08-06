using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/fleet/tracking")]
[Authorize(Roles = AdminRoles)]
public class FleetTrackingController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IFleetTrackingService _trackingService;

    public FleetTrackingController(IFleetTrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    [HttpPost("locations")]
    public async Task<IActionResult> RecordLocation([FromBody] RecordLocationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _trackingService.RecordLocationAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("locations/latest")]
    public async Task<IActionResult> GetLatestLocations(CancellationToken cancellationToken) =>
        Ok(await _trackingService.GetLatestLocationsAsync(cancellationToken));

    [HttpPost("trips/start")]
    public async Task<IActionResult> StartTrip([FromBody] StartTripRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _trackingService.StartTripAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("trips/{id:guid}/end")]
    public async Task<IActionResult> EndTrip(Guid id, [FromBody] EndTripRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _trackingService.EndTripAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("trips")]
    public async Task<IActionResult> GetTrips(
        [FromQuery] Guid? vehicleId,
        [FromQuery] Guid? driverId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var filter = new TripFilter { VehicleId = vehicleId, DriverId = driverId, DateFrom = dateFrom, DateTo = dateTo };
        return Ok(await _trackingService.GetTripsAsync(filter, cancellationToken));
    }

    [HttpGet("trips/{id:guid}/locations")]
    public async Task<IActionResult> GetTripLocations(Guid id, CancellationToken cancellationToken) =>
        Ok(await _trackingService.GetTripLocationsAsync(id, cancellationToken));

    [HttpPost("vehicles/{vehicleId:guid}/simulate")]
    public async Task<IActionResult> SimulateTrip(Guid vehicleId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _trackingService.SimulateTripAsync(vehicleId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
