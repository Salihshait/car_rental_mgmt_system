using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class BillingService : IBillingService
{
    private readonly CarRentDbContext _context;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public BillingService(CarRentDbContext context, IPaymentGatewayService paymentGatewayService)
    {
        _context = context;
        _paymentGatewayService = paymentGatewayService;
    }

    public async Task<SubscriptionInvoiceDto> GenerateInvoiceAsync(GenerateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription not found.");

        var plan = await _context.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == subscription.PlanId, cancellationToken)
            ?? throw new InvalidOperationException("Plan not found.");

        var amount = subscription.BillingCycle == "Annual" ? plan.AnnualPrice : plan.MonthlyPrice;

        var invoice = new SubscriptionInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = subscription.TenantId,
            SubscriptionId = subscription.Id,
            InvoiceNumber = $"SUB-{DateTime.UtcNow:yyyyMMddHHmmss}",
            PeriodStart = subscription.CurrentPeriodStart,
            PeriodEnd = subscription.CurrentPeriodEnd,
            Amount = amount,
            Currency = plan.Currency,
            Status = "Pending"
        };

        await _context.SubscriptionInvoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(invoice, cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionInvoiceDto>> GetAllAsync(Guid? tenantId, string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.SubscriptionInvoices.AsNoTracking().AsQueryable();
        if (tenantId.HasValue) query = query.Where(i => i.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(i => i.Status == status);

        var invoices = await query.OrderByDescending(i => i.CreatedAt).ToListAsync(cancellationToken);
        var tenantIds = invoices.Select(i => i.TenantId).Distinct().ToList();
        var tenants = await _context.Tenants.AsNoTracking().Where(t => tenantIds.Contains(t.Id)).ToListAsync(cancellationToken);

        return invoices.Select(i => MapDto(i, tenants.FirstOrDefault(t => t.Id == i.TenantId)?.CompanyName));
    }

    public async Task<SubscriptionInvoiceDto> MarkPaidAsync(Guid invoiceId, MarkInvoicePaidRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.SubscriptionInvoices.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        var order = await _paymentGatewayService.CreateOrderAsync(request.Gateway, invoice.Amount, invoice.Currency, invoice.InvoiceNumber, cancellationToken);
        var verification = await _paymentGatewayService.VerifyPaymentAsync(request.Gateway, order.OrderId, order.OrderId, null, cancellationToken);

        invoice.Status = verification.IsVerified ? "Paid" : "Failed";
        invoice.PaidAt = verification.IsVerified ? DateTime.UtcNow : null;
        invoice.GatewayReference = order.OrderId;

        await _context.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(invoice, cancellationToken);
    }

    private async Task<SubscriptionInvoiceDto> ToDtoAsync(SubscriptionInvoice invoice, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == invoice.TenantId, cancellationToken);
        return MapDto(invoice, tenant?.CompanyName);
    }

    private static SubscriptionInvoiceDto MapDto(SubscriptionInvoice invoice, string? tenantName) => new(
        invoice.Id, invoice.TenantId, tenantName, invoice.SubscriptionId, invoice.InvoiceNumber, invoice.PeriodStart, invoice.PeriodEnd,
        invoice.Amount, invoice.Currency, invoice.Status, invoice.PaidAt, invoice.GatewayReference, invoice.CreatedAt);
}
