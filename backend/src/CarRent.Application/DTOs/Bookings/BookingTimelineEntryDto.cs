namespace CarRent.Application.DTOs.Bookings;

public class BookingTimelineEntryDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public DateTime CreatedAt { get; set; }
}
