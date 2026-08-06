namespace CarRent.Application.DTOs.Rentals;

public class CreatePickupRequest
{
    public Guid BookingId { get; set; }
    public int OdometerReading { get; set; }
    public decimal FuelLevelPercent { get; set; }
    public string? ConditionNotes { get; set; }
    public decimal? SecurityDepositAmount { get; set; }
}
