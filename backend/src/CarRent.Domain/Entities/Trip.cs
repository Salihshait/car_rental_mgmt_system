namespace CarRent.Domain.Entities;

public class Trip
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }
    public decimal? EndLatitude { get; set; }
    public decimal? EndLongitude { get; set; }
    public decimal DistanceKm { get; set; }
    public string Status { get; set; } = "InProgress";
}
