namespace CarRent.Application.DTOs.Ai;

public record VoiceBookingRequestDto(Guid Id, Guid CustomerId, string TranscribedText, Dictionary<string, string> ParsedIntent, string Status, DateTime CreatedAt);
