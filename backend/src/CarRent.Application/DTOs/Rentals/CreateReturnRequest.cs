namespace CarRent.Application.DTOs.Rentals;

public class CreateReturnRequest
{
    public DateTime? ReturnAt { get; set; }
    public int OdometerReading { get; set; }
    public decimal FuelLevelPercent { get; set; }
    public string? ConditionNotes { get; set; }
}
