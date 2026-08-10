using CarRent.Application.DTOs.Reports;

namespace CarRent.Application.Interfaces;

public interface IReportsMaintenanceService
{
    Task<MaintenanceDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
