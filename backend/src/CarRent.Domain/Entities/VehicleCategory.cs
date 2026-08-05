namespace CarRent.Domain.Entities;

public class VehicleCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
