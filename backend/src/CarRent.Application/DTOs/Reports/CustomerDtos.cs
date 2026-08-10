namespace CarRent.Application.DTOs.Reports;

public record CustomerDetailRowDto(Guid CustomerId, string Name, string Email, bool IsCorporate, bool IsBlacklisted, int BookingCount, decimal TotalSpend);

public record CustomerDashboardDto(
    List<ReportKpiDto> Kpis,
    List<ChartPointDto> NewCustomersTrend,
    List<ChartPointDto> ByType,
    List<ChartPointDto> TopCustomersBySpend,
    List<CustomerDetailRowDto> DetailRows);
