namespace CarRent.Application.DTOs.Bookings;

public class UpdateBookingRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Notes { get; set; }
}
