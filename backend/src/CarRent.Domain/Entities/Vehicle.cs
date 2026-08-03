namespace CarRent.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;
}
