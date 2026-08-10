using CarRent.Application.DTOs.Reports;

namespace CarRent.Application.Interfaces;

public interface IDriverReportService
{
    Task<DriverDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default);
}
