namespace CarRent.Application.DTOs.Fleet;

public class VehicleLocationDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public Guid? TripId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? SpeedKmh { get; set; }
    public decimal? HeadingDegrees { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class RecordLocationRequest
{
    public Guid VehicleId { get; set; }
    public Guid? TripId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? SpeedKmh { get; set; }
    public decimal? HeadingDegrees { get; set; }
}

public class LatestVehicleLocationDto
{
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public string? FleetAvailabilityStatus { get; set; }
    public Guid? ActiveDriverId { get; set; }
    public string? ActiveDriverName { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? SpeedKmh { get; set; }
    public DateTime RecordedAt { get; set; }
}
