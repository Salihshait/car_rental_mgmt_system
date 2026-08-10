using CarRent.Application.DTOs.Finance;
using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services.Reports;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Finance;

public class GstReportService : IGstReportService
{
    private readonly CarRentDbContext _context;

    public GstReportService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<GstSummaryDto> GetSummaryAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var query =
            from i in _context.Invoices.AsNoTracking()
            join b in _context.Bookings.AsNoTracking() on i.BookingId equals b.Id
            where i.IssueDate >= effectiveFrom && i.IssueDate <= effectiveTo
            select new { Invoice = i, Booking = b };

        if (branchId.HasValue)
        {
            query = query.Where(x => x.Booking.BranchId == branchId);
        }

        var rows = await query.ToListAsync(cancellationToken);

        var branchIds = rows.Where(r => r.Booking.BranchId.HasValue).Select(r => r.Booking.BranchId!.Value).Distinct().ToList();
        var branches = await _context.Branches.AsNoTracking().Where(b => branchIds.Contains(b.Id)).ToListAsync(cancellationToken);

        var taxableValue = rows.Sum(r => r.Invoice.SubtotalAmount - r.Invoice.DiscountAmount);
        var cgst = rows.Sum(r => r.Invoice.CgstAmount);
        var sgst = rows.Sum(r => r.Invoice.SgstAmount);
        var igst = rows.Sum(r => r.Invoice.IgstAmount);
        var totalTax = cgst + sgst + igst;

        var kpis = new List<ReportKpiDto>
        {
            new("Taxable Value", taxableValue, "currency", null),
            new("CGST", cgst, "currency", null),
            new("SGST", sgst, "currency", null),
            new("IGST", igst, "currency", null),
            new("Total Tax", totalTax, "currency", null),
            new("Invoices", rows.Count, "number", null)
        };

        var byMonth = rows
            .GroupBy(r => new DateTime(r.Invoice.IssueDate.Year, r.Invoice.IssueDate.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Sum(r => r.Invoice.CgstAmount + r.Invoice.SgstAmount + r.Invoice.IgstAmount)))
            .ToList();

        var byBranch = rows
            .GroupBy(r => r.Booking.BranchId)
            .Select(g => new ChartPointDto(branches.FirstOrDefault(b => b.Id == g.Key)?.Name ?? "Unassigned", g.Sum(r => r.Invoice.CgstAmount + r.Invoice.SgstAmount + r.Invoice.IgstAmount)))
            .OrderByDescending(p => p.Value)
            .ToList();

        var detailRows = rows
            .OrderByDescending(r => r.Invoice.IssueDate)
            .Select(r => new GstDetailRowDto(
                r.Invoice.InvoiceNumber,
                r.Invoice.IssueDate,
                branches.FirstOrDefault(b => b.Id == r.Booking.BranchId)?.Name,
                r.Invoice.SubtotalAmount - r.Invoice.DiscountAmount,
                r.Invoice.CgstAmount,
                r.Invoice.SgstAmount,
                r.Invoice.IgstAmount,
                r.Invoice.CgstAmount + r.Invoice.SgstAmount + r.Invoice.IgstAmount,
                r.Invoice.TotalAmount))
            .ToList();

        return new GstSummaryDto(kpis, byMonth, byBranch, detailRows);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(from, to, branchId, cancellationToken);

        var model = new ReportExportModel(
            "GST Report",
            from,
            to,
            summary.Kpis,
            new List<ReportExportSection>
            {
                new("By Month", new[] { "Period", "Tax" }, summary.ByMonth.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("By Branch", new[] { "Branch", "Tax" }, summary.ByBranch.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Invoices", new[] { "Invoice Number", "Issue Date", "Branch", "Taxable Value", "CGST", "SGST", "IGST", "Total Tax", "Total Amount" },
                    summary.DetailRows.Select(r => new[] { r.InvoiceNumber, r.IssueDate.ToString("d"), r.BranchName ?? "-", r.TaxableValue.ToString("N2"), r.Cgst.ToString("N2"), r.Sgst.ToString("N2"), r.Igst.ToString("N2"), r.TotalTax.ToString("N2"), r.TotalAmount.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
