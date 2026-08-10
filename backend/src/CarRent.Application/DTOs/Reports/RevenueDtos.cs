namespace CarRent.Application.DTOs.Reports;

public record RevenueDetailRowDto(Guid BookingId, DateTime Date, string? BranchName, string? VehicleRegistrationNumber, string Status, decimal Amount);

public record RevenueDashboardDto(
    List<ReportKpiDto> Kpis,
    List<ChartPointDto> Trend,
    List<ChartPointDto> ByBranch,
    List<ChartPointDto> ByVehicleCategory,
    List<RevenueDetailRowDto> DetailRows);
