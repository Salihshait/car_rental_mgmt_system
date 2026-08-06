using CarRent.Application.DTOs.Drivers;

namespace CarRent.Application.Interfaces;

public interface IDriverDocumentService
{
    Task<IEnumerable<DriverDocumentDto>> GetByDriverAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<DriverDocumentDto> CreateAsync(
        Guid driverId, string documentType, string? documentNumber, DateTime? expiryDate, string? storagePath, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task<DriverDocumentDto> VerifyAsync(Guid driverId, Guid documentId, string verificationStatus, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid driverId, Guid documentId, Guid? actingUserId, CancellationToken cancellationToken = default);
}
