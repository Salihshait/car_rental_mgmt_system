namespace CarRent.Application.Interfaces;

public record OcrExtractionResult(Dictionary<string, string> Fields, decimal ConfidenceScore);

public interface IOcrProvider
{
    Task<OcrExtractionResult> ExtractAsync(string documentType, byte[] imageBytes, CancellationToken cancellationToken = default);
}
