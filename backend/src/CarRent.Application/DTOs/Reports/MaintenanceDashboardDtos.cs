namespace CarRent.Application.DTOs.Reports;

public record MaintenanceDetailRowDto(Guid VehicleId, string? RegistrationNumber, decimal TotalCost);

public record MaintenanceDashboardDto(
    List<ReportKpiDto> Kpis,
    List<ChartPointDto> CostByCategory,
    List<ChartPointDto> VendorPerformance,
    List<ChartPointDto> OpenWorkOrdersByType,
    List<MaintenanceDetailRowDto> DetailRows);
