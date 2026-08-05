namespace CarRent.Application.DTOs.VehicleCatalog;

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SaveBrandRequest
{
    public string Name { get; set; } = string.Empty;
}
