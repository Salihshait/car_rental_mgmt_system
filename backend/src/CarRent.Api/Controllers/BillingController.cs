using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/billing")]
[Authorize(Roles = PlatformRoles)]
public class BillingController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? tenantId, [FromQuery] string? status, CancellationToken cancellationToken) =>
        Ok(await _billingService.GetAllAsync(tenantId, status, cancellationToken));

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateInvoiceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _billingService.GenerateInvoiceAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{invoiceId:guid}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid invoiceId, [FromBody] MarkInvoicePaidRequest request, CancellationToken cancellationToken) =>
        Ok(await _billingService.MarkPaidAsync(invoiceId, request, cancellationToken));
}
