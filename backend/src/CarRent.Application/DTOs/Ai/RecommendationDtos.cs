namespace CarRent.Application.DTOs.Ai;

public record VehicleRecommendationDto(Guid VehicleId, string RegistrationNumber, string? BrandName, string? ModelName, decimal DailyRate, string Reason);
