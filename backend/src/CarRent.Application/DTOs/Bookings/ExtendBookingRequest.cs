namespace CarRent.Application.DTOs.Bookings;

public class ExtendBookingRequest
{
    public DateTime NewEndDate { get; set; }
    public string? Reason { get; set; }
}
