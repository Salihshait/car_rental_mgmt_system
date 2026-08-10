using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public record DamageAnalysisResult(List<DamagedAreaDto> Damages, decimal SeverityScore);

public interface IDamageDetectionProvider
{
    Task<DamageAnalysisResult> AnalyzeAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}
