namespace CarRent.Domain.Entities;

public class FuelLog
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime LoggedOn { get; set; } = DateTime.UtcNow;
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
    public int? OdometerReading { get; set; }
    public string LogType { get; set; } = "Refuel";
    public Guid? RecordedBy { get; set; }
}
