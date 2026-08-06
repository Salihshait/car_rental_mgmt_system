namespace CarRent.Application.DTOs.Bookings;

public class BookingFilter
{
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? BranchId { get; set; }
    public string? Status { get; set; }
    public string? BookingType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
