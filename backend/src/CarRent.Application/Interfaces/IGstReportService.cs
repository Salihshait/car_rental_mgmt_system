using CarRent.Application.DTOs.Finance;

namespace CarRent.Application.Interfaces;

public interface IGstReportService
{
    Task<GstSummaryDto> GetSummaryAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default);
}
