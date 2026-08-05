namespace CarRent.Domain.Entities;

public class FavoriteVehicle
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
