namespace CarRent.Domain.Entities;

public class VehicleLocation
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? TripId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? SpeedKmh { get; set; }
    public decimal? HeadingDegrees { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
