namespace CarRent.Application.DTOs.Reports;

public record BookingDetailRowDto(Guid BookingId, DateTime StartDate, DateTime EndDate, string? BranchName, string? VehicleRegistrationNumber, string Status, decimal TotalAmount);

public record BookingsDashboardDto(
    List<ReportKpiDto> Kpis,
    List<ChartPointDto> Trend,
    List<ChartPointDto> ByStatus,
    List<ChartPointDto> ByBranch,
    List<BookingDetailRowDto> DetailRows);
