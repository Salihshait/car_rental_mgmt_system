namespace CarRent.Application.DTOs.Rentals;

public class RentalSummaryDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PickupAt { get; set; }
    public DateTime? ReturnAt { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal SecurityDepositAmount { get; set; }
    public string SecurityDepositStatus { get; set; } = string.Empty;
}
