namespace CarRent.Application.DTOs.Reports;

public record FleetDetailRowDto(Guid VehicleId, string RegistrationNumber, string? BranchName, string Status, int BookingCount, decimal RevenueGenerated);

public record FleetDashboardDto(
    List<ReportKpiDto> Kpis,
    List<ChartPointDto> StatusBreakdown,
    List<ChartPointDto> UtilizationTrend,
    List<ChartPointDto> RevenueByCategory,
    List<FleetDetailRowDto> DetailRows);
