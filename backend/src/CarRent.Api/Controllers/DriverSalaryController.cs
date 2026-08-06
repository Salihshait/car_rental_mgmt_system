using System.Security.Claims;
using CarRent.Application.DTOs.Drivers;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/driver-salary")]
[Authorize(Roles = AdminRoles)]
public class DriverSalaryController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IDriverSalaryService _salaryService;

    public DriverSalaryController(IDriverSalaryService salaryService)
    {
        _salaryService = salaryService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? driverId, CancellationToken cancellationToken) =>
        Ok(await _salaryService.GetAllAsync(driverId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] CreateSalaryPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _salaryService.GenerateAsync(request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _salaryService.MarkPaidAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
