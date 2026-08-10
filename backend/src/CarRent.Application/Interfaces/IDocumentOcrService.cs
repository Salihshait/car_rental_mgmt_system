using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IDocumentOcrService
{
    Task<OcrResultDto> ExtractAsync(string documentType, byte[] imageBytes, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OcrResultDto>> GetHistoryAsync(string? documentType, CancellationToken cancellationToken = default);
}
