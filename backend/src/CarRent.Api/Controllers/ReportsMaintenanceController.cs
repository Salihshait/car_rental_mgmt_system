using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/reports/maintenance")]
[Authorize(Roles = AdminRoles)]
public class ReportsMaintenanceController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IReportsMaintenanceService _reportService;

    public ReportsMaintenanceController(IReportsMaintenanceService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _reportService.GetDashboardAsync(from, to, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportAsync(format, from, to, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "maintenance-report", format, bytes);
    }
}
