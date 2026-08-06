namespace CarRent.Application.DTOs.Bookings;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public string? BrandName { get; set; }
    public string? ModelName { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? ReturnBranchId { get; set; }
    public string BookingType { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public PricingBreakdownDto Pricing { get; set; } = new();
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}
