using CarRent.Application.DTOs.Maintenance;

namespace CarRent.Application.Interfaces;

public interface ISparePartService
{
    Task<IEnumerable<SparePartDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SparePartDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SparePartDto> CreateAsync(SaveSparePartRequest request, CancellationToken cancellationToken = default);
    Task<SparePartDto> UpdateAsync(Guid id, SaveSparePartRequest request, CancellationToken cancellationToken = default);
    Task<SparePartDto> AdjustStockAsync(Guid id, AdjustStockRequest request, CancellationToken cancellationToken = default);

    Task<MaintenancePartUsageDto> RecordUsageAsync(Guid maintenanceId, RecordPartUsageRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenancePartUsageDto>> GetUsageAsync(Guid maintenanceId, CancellationToken cancellationToken = default);
}
