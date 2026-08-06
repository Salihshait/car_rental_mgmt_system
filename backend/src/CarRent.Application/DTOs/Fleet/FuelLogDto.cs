namespace CarRent.Application.DTOs.Fleet;

public class FuelLogDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public DateTime LoggedOn { get; set; }
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
    public int? OdometerReading { get; set; }
    public string LogType { get; set; } = string.Empty;
}

public class CreateFuelLogRequest
{
    public Guid VehicleId { get; set; }
    public DateTime? LoggedOn { get; set; }
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
    public int? OdometerReading { get; set; }
    public string LogType { get; set; } = "Refuel";
}

public class FuelConsumptionSummaryDto
{
    public Guid VehicleId { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public int LogCount { get; set; }
    public decimal? DistancePerUnit { get; set; }
}
