using CarRent.Application.DTOs.Vehicles;

namespace CarRent.Application.Interfaces;

public interface IVehicleDocumentService
{
    Task<IEnumerable<VehicleDocumentDto>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<VehicleDocumentDto> CreateAsync(
        Guid vehicleId,
        string documentType,
        string? documentNumber,
        string? issuedBy,
        DateTime? expiryDate,
        string? storagePath,
        Guid? actingUserId,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid vehicleId, Guid documentId, Guid? actingUserId, CancellationToken cancellationToken = default);
}
