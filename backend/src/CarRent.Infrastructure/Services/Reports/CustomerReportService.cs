using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Reports;

public class CustomerReportService : ICustomerReportService
{
    private static readonly string[] ExcludedBookingStatuses = { "Cancelled", "Rejected" };

    private readonly CarRentDbContext _context;

    public CustomerReportService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var allCustomers = await _context.Customers.AsNoTracking().ToListAsync(cancellationToken);
        var newCustomers = allCustomers.Where(c => c.CreatedAt >= effectiveFrom && c.CreatedAt <= effectiveTo).ToList();

        var userIds = allCustomers.Select(c => c.UserId).ToList();
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);

        var customerIds = allCustomers.Select(c => c.Id).ToList();
        var bookings = await _context.Bookings.AsNoTracking()
            .Where(b => customerIds.Contains(b.CustomerId) && b.StartDate >= effectiveFrom && b.StartDate <= effectiveTo && !ExcludedBookingStatuses.Contains(b.Status))
            .ToListAsync(cancellationToken);

        var totalCustomers = allCustomers.Count;
        var blacklisted = allCustomers.Count(c => c.IsBlacklisted);
        var active = totalCustomers - blacklisted;
        var corporatePercent = totalCustomers == 0 ? 0 : Math.Round(allCustomers.Count(c => c.IsCorporate) / (decimal)totalCustomers * 100, 1);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Customers", totalCustomers, "number", null),
            new("New Customers", newCustomers.Count, "number", null),
            new("Active", active, "number", null),
            new("Blacklisted", blacklisted, "number", null),
            new("Corporate %", corporatePercent, "percent", null)
        };

        var newCustomersTrend = newCustomers
            .GroupBy(c => new DateTime(c.CreatedAt.Year, c.CreatedAt.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Count()))
            .ToList();

        var byType = new List<ChartPointDto>
        {
            new("Corporate", allCustomers.Count(c => c.IsCorporate)),
            new("Individual", allCustomers.Count(c => !c.IsCorporate))
        };

        var spendByCustomer = bookings.GroupBy(b => b.CustomerId).ToDictionary(g => g.Key, g => g.Sum(b => b.TotalAmount));
        var bookingCountByCustomer = bookings.GroupBy(b => b.CustomerId).ToDictionary(g => g.Key, g => g.Count());

        string CustomerName(Guid customerId)
        {
            var customer = allCustomers.FirstOrDefault(c => c.Id == customerId);
            var user = customer is null ? null : users.FirstOrDefault(u => u.Id == customer.UserId);
            return user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim();
        }

        var topCustomers = spendByCustomer
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv => new ChartPointDto(CustomerName(kv.Key), kv.Value))
            .ToList();

        var detailRows = allCustomers
            .Select(c =>
            {
                var user = users.FirstOrDefault(u => u.Id == c.UserId);
                return new CustomerDetailRowDto(
                    c.Id,
                    user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim(),
                    user?.Email ?? "-",
                    c.IsCorporate,
                    c.IsBlacklisted,
                    bookingCountByCustomer.GetValueOrDefault(c.Id),
                    spendByCustomer.GetValueOrDefault(c.Id));
            })
            .OrderByDescending(r => r.TotalSpend)
            .ToList();

        return new CustomerDashboardDto(kpis, newCustomersTrend, byType, topCustomers, detailRows);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(from, to, cancellationToken);

        var model = new ReportExportModel(
            "Customer Report",
            from,
            to,
            dashboard.Kpis,
            new List<ReportExportSection>
            {
                new("New Customers Trend", new[] { "Period", "New Customers" }, dashboard.NewCustomersTrend.Select(p => new[] { p.Key, p.Value.ToString("N0") }).ToList()),
                new("By Type", new[] { "Type", "Count" }, dashboard.ByType.Select(p => new[] { p.Key, p.Value.ToString("N0") }).ToList()),
                new("Top Customers By Spend", new[] { "Customer", "Spend" }, dashboard.TopCustomersBySpend.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Customers", new[] { "Customer Id", "Name", "Email", "Corporate", "Blacklisted", "Bookings", "Total Spend" },
                    dashboard.DetailRows.Select(r => new[] { r.CustomerId.ToString(), r.Name, r.Email, r.IsCorporate.ToString(), r.IsBlacklisted.ToString(), r.BookingCount.ToString(), r.TotalSpend.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
