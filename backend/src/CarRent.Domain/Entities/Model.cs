namespace CarRent.Domain.Entities;

public class Model
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Brand Brand { get; set; } = default!;
    public VehicleCategory Category { get; set; } = default!;
}
