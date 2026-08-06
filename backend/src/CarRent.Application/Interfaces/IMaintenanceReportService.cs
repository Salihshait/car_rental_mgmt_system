using CarRent.Application.DTOs.Maintenance;

namespace CarRent.Application.Interfaces;

public interface IMaintenanceReportService
{
    Task<IEnumerable<MaintenanceCalendarEntryDto>> GetCalendarAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<MaintenanceCostSummaryDto> GetCostSummaryAsync(DateTime? from, DateTime? to, Guid? vehicleId, Guid? workshopId, Guid? vendorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VendorPerformanceDto>> GetVendorPerformanceAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SparePartsConsumptionReportDto> GetSparePartsConsumptionAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
