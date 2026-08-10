namespace CarRent.Application.DTOs.Ai;

public record OcrResultDto(Guid Id, string DocumentType, Dictionary<string, string> ExtractedFields, decimal ConfidenceScore, DateTime CreatedAt);
