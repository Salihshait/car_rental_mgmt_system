using System.Security.Claims;
using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/maintenance-expenses")]
[Authorize(Roles = AdminRoles)]
public class MaintenanceExpensesController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IMaintenanceExpenseService _expenseService;

    public MaintenanceExpensesController(IMaintenanceExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vehicleId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _expenseService.GetAllAsync(vehicleId, from, to, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceExpenseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _expenseService.CreateAsync(request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
