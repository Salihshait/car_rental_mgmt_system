namespace CarRent.Application.DTOs.Bookings;

public class CreateWalkInBookingRequest
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ReturnBranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}
