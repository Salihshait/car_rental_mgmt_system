namespace CarRent.Application.DTOs.Customers;

public class FavoriteVehicleDto
{
    public Guid VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? ModelName { get; set; }
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = string.Empty;
}
