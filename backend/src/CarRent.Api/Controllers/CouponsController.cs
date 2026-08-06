using CarRent.Application.DTOs.Coupons;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CouponsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _couponService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var coupon = await _couponService.GetByIdAsync(id, cancellationToken);
        return coupon is null ? NotFound() : Ok(coupon);
    }

    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromQuery] string code, [FromQuery] decimal subtotalAmount, CancellationToken cancellationToken) =>
        Ok(await _couponService.ValidateAsync(code, subtotalAmount, cancellationToken));

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] CreateCouponRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _couponService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _couponService.UpdateAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _couponService.DeactivateAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
