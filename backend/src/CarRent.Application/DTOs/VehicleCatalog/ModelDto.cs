namespace CarRent.Application.DTOs.VehicleCatalog;

public class ModelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class SaveModelRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
}
