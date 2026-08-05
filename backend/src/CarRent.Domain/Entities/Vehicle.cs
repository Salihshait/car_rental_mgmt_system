namespace CarRent.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string? EngineNumber { get; set; }
    public string? Color { get; set; }
    public int Year { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = "Available";
    public string? GpsDeviceId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? ModelId { get; set; }
    public Guid BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Branch Branch { get; set; } = default!;
    public Brand? Brand { get; set; }
    public Model? VehicleModel { get; set; }
}
