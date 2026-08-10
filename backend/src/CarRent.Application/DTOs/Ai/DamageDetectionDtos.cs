namespace CarRent.Application.DTOs.Ai;

public record DamagedAreaDto(string DamageType, string Location, decimal Confidence);

public record DamageDetectionResultDto(
    Guid Id,
    Guid? VehicleId,
    Guid? RentalId,
    string ImageReference,
    List<DamagedAreaDto> DetectedDamages,
    decimal SeverityScore,
    DateTime CreatedAt);
