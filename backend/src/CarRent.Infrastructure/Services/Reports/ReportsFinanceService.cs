using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Reports;

public class ReportsFinanceService : IReportsFinanceService
{
    private readonly IBillingReportService _billingReportService;
    private readonly CarRentDbContext _context;

    public ReportsFinanceService(IBillingReportService billingReportService, CarRentDbContext context)
    {
        _billingReportService = billingReportService;
        _context = context;
    }

    public async Task<FinanceDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var revenueSummary = await _billingReportService.GetRevenueSummaryAsync(effectiveFrom, effectiveTo, null, cancellationToken);
        var outstandingInvoices = (await _billingReportService.GetOutstandingInvoicesAsync(cancellationToken)).ToList();
        var paymentMethods = (await _billingReportService.GetPaymentMethodBreakdownAsync(effectiveFrom, effectiveTo, cancellationToken)).ToList();

        var invoices = await _context.Invoices.AsNoTracking()
            .Where(i => i.IssueDate >= effectiveFrom && i.IssueDate <= effectiveTo)
            .ToListAsync(cancellationToken);

        var maintenanceExpenses = await _context.MaintenanceExpenses.AsNoTracking()
            .Where(e => e.ExpenseDate >= effectiveFrom && e.ExpenseDate <= effectiveTo)
            .ToListAsync(cancellationToken);
        var vehicleMaintenanceCosts = await _context.VehicleMaintenances.AsNoTracking()
            .Where(m => m.Status == "Completed" && m.ScheduledOn >= effectiveFrom && m.ScheduledOn <= effectiveTo)
            .ToListAsync(cancellationToken);
        var salaryPayments = await _context.DriverSalaryPayments.AsNoTracking()
            .Where(s => s.Status == "Paid" && s.PeriodStart >= effectiveFrom && s.PeriodStart <= effectiveTo)
            .ToListAsync(cancellationToken);

        var totalExpenses = maintenanceExpenses.Sum(e => e.Amount) + vehicleMaintenanceCosts.Sum(m => m.Cost ?? 0) + salaryPayments.Sum(s => s.NetAmount);
        var netProfit = revenueSummary.TotalCollected - totalExpenses;
        var outstandingAmount = outstandingInvoices.Sum(i => i.AmountDue);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Revenue", revenueSummary.TotalCollected, "currency", null),
            new("Total Expenses", totalExpenses, "currency", null),
            new("Net Profit", netProfit, "currency", null),
            new("Outstanding Invoices", outstandingAmount, "currency", null)
        };

        var revenueTrend = invoices
            .GroupBy(i => new DateTime(i.IssueDate.Year, i.IssueDate.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Sum(i => i.AmountPaid)))
            .ToList();

        var expensesByMonth = new Dictionary<DateTime, decimal>();
        void AddExpense(DateTime date, decimal amount)
        {
            var bucket = new DateTime(date.Year, date.Month, 1);
            expensesByMonth[bucket] = expensesByMonth.GetValueOrDefault(bucket) + amount;
        }
        foreach (var e in maintenanceExpenses) AddExpense(e.ExpenseDate, e.Amount);
        foreach (var m in vehicleMaintenanceCosts) AddExpense(m.ScheduledOn, m.Cost ?? 0);
        foreach (var s in salaryPayments) AddExpense(s.PeriodStart, s.NetAmount);

        var expensesTrend = expensesByMonth
            .OrderBy(kv => kv.Key)
            .Select(kv => new ChartPointDto(kv.Key.ToString("MMM yyyy"), kv.Value))
            .ToList();

        var paymentMethodsChart = paymentMethods
            .Select(p => new ChartPointDto(p.Gateway, p.TotalAmount))
            .OrderByDescending(p => p.Value)
            .ToList();

        var detailRows = outstandingInvoices
            .OrderByDescending(i => i.DaysOutstanding)
            .Select(i => new FinanceDetailRowDto(i.InvoiceNumber, i.IssueDate, i.TotalAmount, i.AmountDue, i.DaysOutstanding))
            .ToList();

        return new FinanceDashboardDto(kpis, revenueTrend, expensesTrend, paymentMethodsChart, detailRows);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(from, to, cancellationToken);

        var model = new ReportExportModel(
            "Finance Report",
            from,
            to,
            dashboard.Kpis,
            new List<ReportExportSection>
            {
                new("Revenue Trend", new[] { "Period", "Revenue" }, dashboard.RevenueTrend.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Expenses Trend", new[] { "Period", "Expenses" }, dashboard.ExpensesTrend.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Payment Methods", new[] { "Gateway", "Amount" }, dashboard.PaymentMethods.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Outstanding Invoices", new[] { "Invoice Number", "Issue Date", "Total", "Due", "Days Outstanding" },
                    dashboard.DetailRows.Select(r => new[] { r.InvoiceNumber, r.IssueDate.ToString("d"), r.TotalAmount.ToString("N2"), r.AmountDue.ToString("N2"), r.DaysOutstanding.ToString() }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
