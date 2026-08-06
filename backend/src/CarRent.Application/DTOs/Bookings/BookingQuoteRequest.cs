namespace CarRent.Application.DTOs.Bookings;

public class BookingQuoteRequest
{
    public Guid VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CouponCode { get; set; }
}
