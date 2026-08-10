using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface IUsageMonitoringService
{
    Task RecordMetricAsync(Guid tenantId, RecordUsageMetricRequest request, CancellationToken cancellationToken = default);
    Task<PlatformOverviewDto> GetPlatformOverviewAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<TenantUsageDto> GetTenantUsageAsync(Guid tenantId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
