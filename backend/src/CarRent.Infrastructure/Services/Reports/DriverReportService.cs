using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Reports;

public class DriverReportService : IDriverReportService
{
    private readonly CarRentDbContext _context;

    public DriverReportService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<DriverDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var driverQuery = _context.Drivers.AsNoTracking().AsQueryable();
        if (branchId.HasValue)
        {
            driverQuery = driverQuery.Where(d => d.BranchId == branchId);
        }
        var drivers = await driverQuery.ToListAsync(cancellationToken);

        var userIds = drivers.Select(d => d.UserId).ToList();
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);

        var driverIds = drivers.Select(d => d.Id).ToList();
        var attendance = await _context.DriverAttendances.AsNoTracking()
            .Where(a => driverIds.Contains(a.DriverId) && a.AttendanceDate >= effectiveFrom && a.AttendanceDate <= effectiveTo)
            .ToListAsync(cancellationToken);
        var salaryPayments = await _context.DriverSalaryPayments.AsNoTracking()
            .Where(s => driverIds.Contains(s.DriverId) && s.PeriodStart >= effectiveFrom && s.PeriodStart <= effectiveTo)
            .ToListAsync(cancellationToken);

        var totalDrivers = drivers.Count;
        var activeDrivers = drivers.Count(d => d.EmploymentStatus == "Active");
        var ratedDrivers = drivers.Where(d => d.Rating.HasValue).ToList();
        var avgRating = ratedDrivers.Count == 0 ? 0 : Math.Round(ratedDrivers.Average(d => d.Rating!.Value), 2);
        var totalSalaryPaid = salaryPayments.Where(s => s.Status == "Paid").Sum(s => s.NetAmount);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Drivers", totalDrivers, "number", null),
            new("Active", activeDrivers, "number", null),
            new("Avg Rating", avgRating, "number", null),
            new("Salary Paid", totalSalaryPaid, "currency", null)
        };

        var ratingDistribution = new[] { "1-2", "2-3", "3-4", "4-5" }
            .Select(bucket =>
            {
                var (lower, upper) = bucket switch
                {
                    "1-2" => (1m, 2m),
                    "2-3" => (2m, 3m),
                    "3-4" => (3m, 4m),
                    _ => (4m, 5m)
                };
                var count = ratedDrivers.Count(d => d.Rating >= lower && (d.Rating < upper || (upper == 5 && d.Rating == 5)));
                return new ChartPointDto(bucket, count);
            })
            .ToList();

        var attendanceTrend = attendance
            .GroupBy(a => new DateTime(a.AttendanceDate.Year, a.AttendanceDate.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var total = g.Count();
                var present = g.Count(a => a.Status == "Present");
                var rate = total == 0 ? 0 : Math.Round(present / (decimal)total * 100, 1);
                return new ChartPointDto(g.Key.ToString("MMM yyyy"), rate);
            })
            .ToList();

        var salaryByMonth = salaryPayments
            .GroupBy(s => new DateTime(s.PeriodStart.Year, s.PeriodStart.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Sum(s => s.NetAmount)))
            .ToList();

        var attendanceByDriver = attendance
            .GroupBy(a => a.DriverId)
            .ToDictionary(g => g.Key, g => (Total: g.Count(), Present: g.Count(a => a.Status == "Present")));
        var salaryByDriver = salaryPayments
            .Where(s => s.Status == "Paid")
            .GroupBy(s => s.DriverId)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.NetAmount));

        var detailRows = drivers
            .Select(d =>
            {
                var user = users.FirstOrDefault(u => u.Id == d.UserId);
                var (total, present) = attendanceByDriver.GetValueOrDefault(d.Id, (0, 0));
                var rate = total == 0 ? 0 : Math.Round(present / (decimal)total * 100, 1);
                return new DriverDetailRowDto(
                    d.Id,
                    user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim(),
                    d.EmploymentStatus,
                    d.Rating,
                    rate,
                    salaryByDriver.GetValueOrDefault(d.Id));
            })
            .OrderByDescending(r => r.SalaryPaid)
            .ToList();

        return new DriverDashboardDto(kpis, ratingDistribution, attendanceTrend, salaryByMonth, detailRows);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(from, to, branchId, cancellationToken);

        var model = new ReportExportModel(
            "Driver Report",
            from,
            to,
            dashboard.Kpis,
            new List<ReportExportSection>
            {
                new("Rating Distribution", new[] { "Range", "Drivers" }, dashboard.RatingDistribution.Select(p => new[] { p.Key, p.Value.ToString("N0") }).ToList()),
                new("Attendance Trend", new[] { "Period", "Attendance Rate %" }, dashboard.AttendanceTrend.Select(p => new[] { p.Key, p.Value.ToString("N1") }).ToList()),
                new("Salary Payout By Month", new[] { "Period", "Amount" }, dashboard.SalaryPayoutByMonth.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Drivers", new[] { "Driver Id", "Name", "Status", "Rating", "Attendance Rate %", "Salary Paid" },
                    dashboard.DetailRows.Select(r => new[] { r.DriverId.ToString(), r.Name, r.EmploymentStatus, r.Rating?.ToString("N1") ?? "-", r.AttendanceRate.ToString("N1"), r.SalaryPaid.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
