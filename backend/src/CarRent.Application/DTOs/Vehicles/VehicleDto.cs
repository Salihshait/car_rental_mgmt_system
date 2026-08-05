namespace CarRent.Application.DTOs.Vehicles;

public class VehicleDto
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
    public string Status { get; set; } = string.Empty;
    public string? GpsDeviceId { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? ModelId { get; set; }
    public string? ModelName { get; set; }
    public string? CategoryName { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
