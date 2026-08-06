using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/maintenance/reports")]
[Authorize(Roles = AdminRoles)]
public class MaintenanceReportsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IMaintenanceReportService _reportService;

    public MaintenanceReportsController(IMaintenanceReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendar([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken) =>
        Ok(await _reportService.GetCalendarAsync(from, to, cancellationToken));

    [HttpGet("cost-summary")]
    public async Task<IActionResult> GetCostSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? vehicleId,
        [FromQuery] Guid? workshopId,
        [FromQuery] Guid? vendorId,
        CancellationToken cancellationToken) =>
        Ok(await _reportService.GetCostSummaryAsync(from, to, vehicleId, workshopId, vendorId, cancellationToken));

    [HttpGet("vendor-performance")]
    public async Task<IActionResult> GetVendorPerformance([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _reportService.GetVendorPerformanceAsync(from, to, cancellationToken));

    [HttpGet("spare-parts-consumption")]
    public async Task<IActionResult> GetSparePartsConsumption([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _reportService.GetSparePartsConsumptionAsync(from, to, cancellationToken));
}
