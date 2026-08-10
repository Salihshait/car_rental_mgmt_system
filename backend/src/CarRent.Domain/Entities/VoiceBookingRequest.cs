namespace CarRent.Domain.Entities;

public class VoiceBookingRequest
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string TranscribedText { get; set; } = string.Empty;
    public string ParsedIntentJson { get; set; } = "{}";
    public string Status { get; set; } = "Transcribed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
