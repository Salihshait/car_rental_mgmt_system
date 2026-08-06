using CarRent.Application.DTOs.Fleet;

namespace CarRent.Application.Interfaces;

public interface IFleetTrackingService
{
    Task<VehicleLocationDto> RecordLocationAsync(RecordLocationRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<LatestVehicleLocationDto>> GetLatestLocationsAsync(CancellationToken cancellationToken = default);

    Task<TripDto> StartTripAsync(StartTripRequest request, CancellationToken cancellationToken = default);
    Task<TripDto> EndTripAsync(Guid tripId, EndTripRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TripDto>> GetTripsAsync(TripFilter filter, CancellationToken cancellationToken = default);
    Task<IEnumerable<VehicleLocationDto>> GetTripLocationsAsync(Guid tripId, CancellationToken cancellationToken = default);

    /// <summary>Demo-only: starts a trip near the vehicle's branch, generates a short random-walk of pings, and ends it.</summary>
    Task<TripDto> SimulateTripAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
