namespace CarRent.Application.DTOs.Ai;

public record MaintenancePredictionDto(
    Guid Id,
    Guid VehicleId,
    string? VehicleRegistrationNumber,
    string PredictedIssue,
    DateTime PredictedDueDate,
    decimal ConfidenceScore,
    string BasisSummary,
    string Status,
    DateTime CreatedAt);

public record UpdatePredictionStatusRequest(string Status);
