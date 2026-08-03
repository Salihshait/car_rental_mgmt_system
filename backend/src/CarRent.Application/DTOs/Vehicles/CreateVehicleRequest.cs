namespace CarRent.Application.DTOs.Vehicles;

public class CreateVehicleRequest
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public decimal DailyRate { get; set; }
    public Guid BranchId { get; set; }
}
