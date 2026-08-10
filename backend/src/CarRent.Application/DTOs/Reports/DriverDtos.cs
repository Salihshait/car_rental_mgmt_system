namespace CarRent.Application.DTOs.Reports;

public record DriverDetailRowDto(Guid DriverId, string Name, string EmploymentStatus, decimal? Rating, decimal AttendanceRate, decimal SalaryPaid);

public record DriverDashboardDto(
    List<ReportKpiDto> Kpis,
    List<ChartPointDto> RatingDistribution,
    List<ChartPointDto> AttendanceTrend,
    List<ChartPointDto> SalaryPayoutByMonth,
    List<DriverDetailRowDto> DetailRows);
