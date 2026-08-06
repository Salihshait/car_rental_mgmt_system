using System.Security.Claims;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.DTOs.Customers;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class AddFavoriteRequest
{
    public Guid VehicleId { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly ICustomerService _customerService;
    private readonly IBookingService _bookingService;

    public CustomersController(ICustomerService customerService, IBookingService bookingService)
    {
        _customerService = customerService;
        _bookingService = bookingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? kycStatus,
        [FromQuery] bool? isBlacklisted,
        [FromQuery] bool? isCorporate,
        CancellationToken cancellationToken)
    {
        var filter = new CustomerFilter { Search = search, KycStatus = kycStatus, IsBlacklisted = isBlacklisted, IsCorporate = isCorporate };
        return Ok(await _customerService.GetAllAsync(filter, cancellationToken));
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await _customerService.GetDashboardAsync(cancellationToken));

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(CurrentUserId, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateEmergencyContactRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _customerService.UpdateEmergencyContactAsync(CurrentUserId, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me/bookings")]
    public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetAllAsync(new BookingFilter { CustomerId = CurrentUserId }, CurrentUserId, isAdmin: false, cancellationToken));

    [HttpGet("me/favorites")]
    public async Task<IActionResult> GetMyFavorites(CancellationToken cancellationToken) =>
        Ok(await _customerService.GetFavoritesAsync(CurrentUserId, cancellationToken));

    [HttpPost("me/favorites")]
    public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.AddFavoriteAsync(CurrentUserId, request.VehicleId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("me/favorites/{vehicleId:guid}")]
    public async Task<IActionResult> RemoveFavorite(Guid vehicleId, CancellationToken cancellationToken)
    {
        await _customerService.RemoveFavoriteAsync(CurrentUserId, vehicleId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("{id:guid}/bookings")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetBookings(Guid id, CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetAllAsync(new BookingFilter { CustomerId = id }, CurrentUserId, isAdmin: true, cancellationToken));

    [HttpGet("{id:guid}/timeline")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken cancellationToken) =>
        Ok(await _customerService.GetTimelineAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _customerService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _customerService.UpdateAsync(id, request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/wallet/adjust")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> AdjustWallet(Guid id, [FromBody] WalletAdjustRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _customerService.AdjustWalletAsync(id, request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/loyalty/adjust")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> AdjustLoyalty(Guid id, [FromBody] LoyaltyAdjustRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _customerService.AdjustLoyaltyAsync(id, request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
