namespace CarRent.Application.DTOs.VehicleCatalog;

public class VehicleCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class SaveVehicleCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
