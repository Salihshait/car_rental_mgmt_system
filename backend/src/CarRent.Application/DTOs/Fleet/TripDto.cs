namespace CarRent.Application.DTOs.Fleet;

public class TripDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }
    public decimal? EndLatitude { get; set; }
    public decimal? EndLongitude { get; set; }
    public decimal DistanceKm { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StartTripRequest
{
    public Guid VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }
}

public class EndTripRequest
{
    public decimal? EndLatitude { get; set; }
    public decimal? EndLongitude { get; set; }
}

public class TripFilter
{
    public Guid? VehicleId { get; set; }
    public Guid? DriverId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
