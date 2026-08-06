using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/billing/reports")]
[Authorize(Roles = AdminRoles)]
public class BillingReportsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IBillingReportService _reportService;

    public BillingReportsController(IBillingReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("revenue-summary")]
    public async Task<IActionResult> GetRevenueSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId, CancellationToken cancellationToken) =>
        Ok(await _reportService.GetRevenueSummaryAsync(from, to, branchId, cancellationToken));

    [HttpGet("outstanding-invoices")]
    public async Task<IActionResult> GetOutstandingInvoices(CancellationToken cancellationToken) =>
        Ok(await _reportService.GetOutstandingInvoicesAsync(cancellationToken));

    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _reportService.GetPaymentMethodBreakdownAsync(from, to, cancellationToken));
}
