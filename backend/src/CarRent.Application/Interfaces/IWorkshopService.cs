using CarRent.Application.DTOs.Maintenance;

namespace CarRent.Application.Interfaces;

public interface IWorkshopService
{
    Task<IEnumerable<WorkshopDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WorkshopDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkshopDto> CreateAsync(SaveWorkshopRequest request, CancellationToken cancellationToken = default);
    Task<WorkshopDto> UpdateAsync(Guid id, SaveWorkshopRequest request, CancellationToken cancellationToken = default);
}
