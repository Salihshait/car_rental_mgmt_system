using CarRent.Application.DTOs.Billing;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class BillingReportService : IBillingReportService
{
    private readonly CarRentDbContext _context;

    public BillingReportService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-1);
        var effectiveTo = to ?? DateTime.UtcNow;

        var invoiceQuery =
            from i in _context.Invoices.AsNoTracking()
            join b in _context.Bookings.AsNoTracking() on i.BookingId equals b.Id
            where i.IssueDate >= effectiveFrom && i.IssueDate <= effectiveTo
            select new { Invoice = i, Booking = b };

        if (branchId.HasValue)
        {
            invoiceQuery = invoiceQuery.Where(x => x.Booking.BranchId == branchId);
        }

        var invoices = await invoiceQuery.Select(x => x.Invoice).ToListAsync(cancellationToken);

        var refundsTotal = await _context.Refunds.AsNoTracking()
            .Where(r => r.Status == "Processed" && r.RequestedAt >= effectiveFrom && r.RequestedAt <= effectiveTo)
            .SumAsync(r => (decimal?)r.Amount, cancellationToken) ?? 0;

        var depositsHeld = await _context.Rentals.AsNoTracking()
            .Where(r => r.SecurityDepositStatus == "Held")
            .SumAsync(r => (decimal?)r.SecurityDepositAmount, cancellationToken) ?? 0;

        var totalInvoiced = invoices.Sum(i => i.TotalAmount);
        var totalCollected = invoices.Sum(i => i.AmountPaid);

        return new RevenueSummaryDto
        {
            TotalInvoiced = totalInvoiced,
            TotalCollected = totalCollected,
            TotalOutstanding = totalInvoiced - totalCollected,
            TotalDiscountGiven = invoices.Sum(i => i.DiscountAmount),
            CgstCollected = invoices.Sum(i => i.CgstAmount),
            SgstCollected = invoices.Sum(i => i.SgstAmount),
            IgstCollected = invoices.Sum(i => i.IgstAmount),
            TotalRefundsIssued = refundsTotal,
            TotalSecurityDepositsHeld = depositsHeld,
            InvoiceCount = invoices.Count
        };
    }

    public async Task<IEnumerable<OutstandingInvoiceDto>> GetOutstandingInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices.AsNoTracking()
            .Where(i => i.Status == "Unpaid" || i.Status == "PartiallyPaid")
            .OrderBy(i => i.IssueDate)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        return invoices.Select(i => new OutstandingInvoiceDto
        {
            InvoiceId = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            BookingId = i.BookingId,
            TotalAmount = i.TotalAmount,
            AmountPaid = i.AmountPaid,
            AmountDue = i.TotalAmount - i.AmountPaid,
            Status = i.Status,
            IssueDate = i.IssueDate,
            DaysOutstanding = Math.Max(0, (int)(now - i.IssueDate).TotalDays)
        }).ToList();
    }

    public async Task<IEnumerable<PaymentMethodBreakdownDto>> GetPaymentMethodBreakdownAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-1);
        var effectiveTo = to ?? DateTime.UtcNow;

        var payments = await _context.Payments.AsNoTracking()
            .Where(p => p.Status == "Verified" && p.PaidAt >= effectiveFrom && p.PaidAt <= effectiveTo)
            .ToListAsync(cancellationToken);

        return payments
            .GroupBy(p => p.Gateway)
            .Select(g => new PaymentMethodBreakdownDto
            {
                Gateway = g.Key,
                PaymentCount = g.Count(),
                TotalAmount = g.Sum(p => p.Amount)
            })
            .OrderByDescending(d => d.TotalAmount)
            .ToList();
    }
}
