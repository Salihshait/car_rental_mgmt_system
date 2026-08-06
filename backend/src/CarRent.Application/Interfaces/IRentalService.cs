using CarRent.Application.DTOs.Rentals;

namespace CarRent.Application.Interfaces;

public interface IRentalService
{
    Task<IEnumerable<RentalSummaryDto>> GetAllAsync(RentalFilter filter, CancellationToken cancellationToken = default);
    Task<RentalDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RentalDto?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

    Task<RentalDto> PickupAsync(CreatePickupRequest request, Guid staffUserId, CancellationToken cancellationToken = default);

    /// <summary>Builds the agreement PDF bytes for the given rental; the controller uploads the result and calls CompleteAgreementAsync.</summary>
    Task<byte[]> GenerateAgreementPdfAsync(Guid rentalId, byte[] signatureImageBytes, CancellationToken cancellationToken = default);
    Task<RentalDto> CompleteAgreementAsync(Guid rentalId, string signatureUrl, string pdfUrl, CancellationToken cancellationToken = default);

    Task<IEnumerable<RentalDamageDto>> GetDamagesAsync(Guid rentalId, CancellationToken cancellationToken = default);
    Task<RentalDamageDto> AddDamageAsync(Guid rentalId, CreateRentalDamageRequest request, Guid reportedBy, CancellationToken cancellationToken = default);

    Task<IEnumerable<RentalPhotoDto>> GetPhotosAsync(Guid rentalId, CancellationToken cancellationToken = default);
    Task<RentalPhotoDto> AddPhotoAsync(Guid rentalId, string stage, string category, string storageUrl, Guid? rentalDamageId, Guid uploadedBy, CancellationToken cancellationToken = default);

    Task<IEnumerable<RentalChargeDto>> GetChargesAsync(Guid rentalId, CancellationToken cancellationToken = default);
    Task<RentalChargeDto> AddChargeAsync(Guid rentalId, CreateRentalChargeRequest request, CancellationToken cancellationToken = default);

    Task<RentalDto> ReturnAsync(Guid rentalId, CreateReturnRequest request, Guid staffUserId, CancellationToken cancellationToken = default);

    Task<RentalReportSummaryDto> GetReportSummaryAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default);
}
