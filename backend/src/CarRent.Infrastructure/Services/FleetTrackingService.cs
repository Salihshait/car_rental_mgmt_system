using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class FleetTrackingService : IFleetTrackingService
{
    private const decimal DefaultLatitude = 28.6139m;
    private const decimal DefaultLongitude = 77.2090m;

    private readonly CarRentDbContext _context;
    private readonly Random _random = new();

    public FleetTrackingService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleLocationDto> RecordLocationAsync(RecordLocationRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        if (request.TripId.HasValue && !await _context.Trips.AnyAsync(t => t.Id == request.TripId && t.VehicleId == vehicle.Id, cancellationToken))
        {
            throw new InvalidOperationException("The referenced trip does not belong to this vehicle.");
        }

        var location = new VehicleLocation
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            TripId = request.TripId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SpeedKmh = request.SpeedKmh,
            HeadingDegrees = request.HeadingDegrees
        };

        await _context.VehicleLocations.AddAsync(location, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleLocationDto
        {
            Id = location.Id,
            VehicleId = location.VehicleId,
            VehicleRegistrationNumber = vehicle.RegistrationNumber,
            TripId = location.TripId,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            SpeedKmh = location.SpeedKmh,
            HeadingDegrees = location.HeadingDegrees,
            RecordedAt = location.RecordedAt
        };
    }

    public async Task<IEnumerable<LatestVehicleLocationDto>> GetLatestLocationsAsync(CancellationToken cancellationToken = default)
    {
        var latestIds = await _context.VehicleLocations
            .GroupBy(l => l.VehicleId)
            .Select(g => g.OrderByDescending(l => l.RecordedAt).Select(l => l.Id).First())
            .ToListAsync(cancellationToken);

        var latestLocations = await _context.VehicleLocations
            .AsNoTracking()
            .Where(l => latestIds.Contains(l.Id))
            .ToListAsync(cancellationToken);

        var vehicleIds = latestLocations.Select(l => l.VehicleId).ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        // Rentals key off BookingId, not VehicleId directly - resolve via Bookings.
        var activeBookingVehicleIds = await (
            from r in _context.Rentals.AsNoTracking()
            join b in _context.Bookings.AsNoTracking() on r.BookingId equals b.Id
            where r.Status == "Active" && vehicleIds.Contains(b.VehicleId)
            select b.VehicleId).ToListAsync(cancellationToken);

        var inMaintenanceVehicleIds = await _context.VehicleMaintenances.AsNoTracking()
            .Where(m => m.Status == "InProgress" && vehicleIds.Contains(m.VehicleId))
            .Select(m => m.VehicleId)
            .ToListAsync(cancellationToken);

        var inTransitVehicleIds = await _context.VehicleTransfers.AsNoTracking()
            .Where(t => t.Status == "InTransit" && vehicleIds.Contains(t.VehicleId))
            .Select(t => t.VehicleId)
            .ToListAsync(cancellationToken);

        var activeAssignments = await _context.DriverAssignments.AsNoTracking()
            .Where(a => a.UnassignedAt == null && vehicleIds.Contains(a.VehicleId))
            .ToListAsync(cancellationToken);

        var driverIds = activeAssignments.Select(a => a.DriverId).ToList();
        var drivers = await _context.Drivers.AsNoTracking().Where(d => driverIds.Contains(d.Id)).ToListAsync(cancellationToken);
        var driverUserIds = drivers.Select(d => d.UserId).ToList();
        var driverUsers = await _context.Users.AsNoTracking().Where(u => driverUserIds.Contains(u.Id)).ToListAsync(cancellationToken);

        return latestLocations.Select(l =>
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == l.VehicleId);
            var assignment = activeAssignments.FirstOrDefault(a => a.VehicleId == l.VehicleId);
            var driver = assignment is null ? null : drivers.FirstOrDefault(d => d.Id == assignment.DriverId);
            var driverUser = driver is null ? null : driverUsers.FirstOrDefault(u => u.Id == driver.UserId);

            var status = FleetStatusHelper.Compute(
                vehicle?.Status ?? "Available",
                activeBookingVehicleIds.Contains(l.VehicleId),
                inMaintenanceVehicleIds.Contains(l.VehicleId),
                inTransitVehicleIds.Contains(l.VehicleId));

            return new LatestVehicleLocationDto
            {
                VehicleId = l.VehicleId,
                VehicleRegistrationNumber = vehicle?.RegistrationNumber,
                FleetAvailabilityStatus = status,
                ActiveDriverId = driver?.Id,
                ActiveDriverName = driverUser is null ? null : $"{driverUser.FirstName} {driverUser.LastName}",
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                SpeedKmh = l.SpeedKmh,
                RecordedAt = l.RecordedAt
            };
        }).ToList();
    }

    public async Task<TripDto> StartTripAsync(StartTripRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        if (request.DriverId.HasValue && !await _context.Drivers.AnyAsync(d => d.Id == request.DriverId, cancellationToken))
        {
            throw new InvalidOperationException("Driver not found.");
        }

        if (await _context.Trips.AnyAsync(t => t.VehicleId == vehicle.Id && t.Status == "InProgress", cancellationToken))
        {
            throw new InvalidOperationException("This vehicle already has a trip in progress.");
        }

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            DriverId = request.DriverId,
            StartLatitude = request.StartLatitude,
            StartLongitude = request.StartLongitude,
            Status = "InProgress"
        };

        await _context.Trips.AddAsync(trip, cancellationToken);

        await _context.VehicleLocations.AddAsync(new VehicleLocation
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            TripId = trip.Id,
            Latitude = request.StartLatitude,
            Longitude = request.StartLongitude,
            RecordedAt = trip.StartedAt
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return await MapTripAsync(trip, cancellationToken);
    }

    public async Task<TripDto> EndTripAsync(Guid tripId, EndTripRequest request, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId, cancellationToken)
            ?? throw new InvalidOperationException("Trip not found.");

        if (trip.Status != "InProgress")
        {
            throw new InvalidOperationException("Only in-progress trips can be ended.");
        }

        var pings = await _context.VehicleLocations
            .AsNoTracking()
            .Where(l => l.TripId == tripId)
            .OrderBy(l => l.RecordedAt)
            .ToListAsync(cancellationToken);

        decimal distanceKm = 0;
        for (var i = 1; i < pings.Count; i++)
        {
            distanceKm += (decimal)FleetStatusHelper.HaversineKm(
                (double)pings[i - 1].Latitude, (double)pings[i - 1].Longitude,
                (double)pings[i].Latitude, (double)pings[i].Longitude);
        }

        var lastPing = pings.LastOrDefault();

        trip.EndedAt = DateTime.UtcNow;
        trip.EndLatitude = request.EndLatitude ?? lastPing?.Latitude ?? trip.StartLatitude;
        trip.EndLongitude = request.EndLongitude ?? lastPing?.Longitude ?? trip.StartLongitude;
        trip.DistanceKm = Math.Round(distanceKm, 2);
        trip.Status = "Completed";

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == trip.VehicleId, cancellationToken);
        if (vehicle is not null && trip.DistanceKm > 0)
        {
            vehicle.CurrentOdometerReading = (vehicle.CurrentOdometerReading ?? 0) + (int)Math.Round(trip.DistanceKm);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await MapTripAsync(trip, cancellationToken);
    }

    public async Task<IEnumerable<TripDto>> GetTripsAsync(TripFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Trips.AsNoTracking().AsQueryable();

        if (filter.VehicleId.HasValue)
        {
            query = query.Where(t => t.VehicleId == filter.VehicleId);
        }

        if (filter.DriverId.HasValue)
        {
            query = query.Where(t => t.DriverId == filter.DriverId);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(t => t.StartedAt >= filter.DateFrom);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(t => t.StartedAt <= filter.DateTo);
        }

        var trips = await query.OrderByDescending(t => t.StartedAt).ToListAsync(cancellationToken);
        return await MapTripsAsync(trips, cancellationToken);
    }

    public async Task<IEnumerable<VehicleLocationDto>> GetTripLocationsAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.VehicleLocations
            .AsNoTracking()
            .Where(l => l.TripId == tripId)
            .OrderBy(l => l.RecordedAt)
            .Select(l => new VehicleLocationDto
            {
                Id = l.Id,
                VehicleId = l.VehicleId,
                TripId = l.TripId,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                SpeedKmh = l.SpeedKmh,
                HeadingDegrees = l.HeadingDegrees,
                RecordedAt = l.RecordedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TripDto> SimulateTripAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.Include(v => v.Branch).FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var driverId = await _context.DriverAssignments.AsNoTracking()
            .Where(a => a.VehicleId == vehicleId && a.UnassignedAt == null)
            .Select(a => (Guid?)a.DriverId)
            .FirstOrDefaultAsync(cancellationToken);

        var baseLat = vehicle.Branch?.Latitude ?? DefaultLatitude;
        var baseLon = vehicle.Branch?.Longitude ?? DefaultLongitude;

        var trip = await StartTripAsync(new StartTripRequest
        {
            VehicleId = vehicleId,
            DriverId = driverId,
            StartLatitude = baseLat,
            StartLongitude = baseLon
        }, cancellationToken);

        var lat = baseLat;
        var lon = baseLon;
        var pingCount = _random.Next(8, 13);

        for (var i = 0; i < pingCount; i++)
        {
            lat += (decimal)(_random.NextDouble() - 0.5) * 0.004m;
            lon += (decimal)(_random.NextDouble() - 0.5) * 0.004m;

            await RecordLocationAsync(new RecordLocationRequest
            {
                VehicleId = vehicleId,
                TripId = trip.Id,
                Latitude = lat,
                Longitude = lon,
                SpeedKmh = _random.Next(20, 80)
            }, cancellationToken);
        }

        return await EndTripAsync(trip.Id, new EndTripRequest { EndLatitude = lat, EndLongitude = lon }, cancellationToken);
    }

    private async Task<TripDto> MapTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        var mapped = await MapTripsAsync(new List<Trip> { trip }, cancellationToken);
        return mapped.First();
    }

    private async Task<List<TripDto>> MapTripsAsync(List<Trip> trips, CancellationToken cancellationToken)
    {
        var vehicleIds = trips.Select(t => t.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var driverIds = trips.Where(t => t.DriverId.HasValue).Select(t => t.DriverId!.Value).Distinct().ToList();
        var drivers = await _context.Drivers.AsNoTracking().Where(d => driverIds.Contains(d.Id)).ToListAsync(cancellationToken);
        var driverUserIds = drivers.Select(d => d.UserId).ToList();
        var driverUsers = await _context.Users.AsNoTracking().Where(u => driverUserIds.Contains(u.Id)).ToListAsync(cancellationToken);

        return trips.Select(t =>
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == t.VehicleId);
            var driver = t.DriverId.HasValue ? drivers.FirstOrDefault(d => d.Id == t.DriverId) : null;
            var driverUser = driver is null ? null : driverUsers.FirstOrDefault(u => u.Id == driver.UserId);

            return new TripDto
            {
                Id = t.Id,
                VehicleId = t.VehicleId,
                VehicleRegistrationNumber = vehicle?.RegistrationNumber,
                DriverId = t.DriverId,
                DriverName = driverUser is null ? null : $"{driverUser.FirstName} {driverUser.LastName}",
                StartedAt = t.StartedAt,
                EndedAt = t.EndedAt,
                StartLatitude = t.StartLatitude,
                StartLongitude = t.StartLongitude,
                EndLatitude = t.EndLatitude,
                EndLongitude = t.EndLongitude,
                DistanceKm = t.DistanceKm,
                Status = t.Status
            };
        }).ToList();
    }
}
