namespace CarRent.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Confirmed";
    public string? Notes { get; set; }
}
