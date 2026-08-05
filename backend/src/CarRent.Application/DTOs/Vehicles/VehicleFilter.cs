namespace CarRent.Application.DTOs.Vehicles;

public class VehicleFilter
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? BranchId { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
}
