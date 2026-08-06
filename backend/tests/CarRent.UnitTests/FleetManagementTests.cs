using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRent.UnitTests;

public class FleetManagementTests
{
    private static CarRentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CarRentDbContext(options);
    }

    [Fact]
    public void HaversineKm_ReturnsKnownDistance_BetweenLondonAndParis()
    {
        // London (51.5074, -0.1278) to Paris (48.8566, 2.3522) is ~344 km.
        var distance = FleetStatusHelper.HaversineKm(51.5074, -0.1278, 48.8566, 2.3522);

        Assert.InRange(distance, 340, 348);
    }

    [Fact]
    public void HaversineKm_ReturnsZero_ForSamePoint()
    {
        var distance = FleetStatusHelper.HaversineKm(28.6139, 77.2090, 28.6139, 77.2090);
        Assert.Equal(0, distance, 3);
    }

    [Theory]
    [InlineData(true, true, true, "Rented")]
    [InlineData(false, true, true, "InMaintenance")]
    [InlineData(false, false, true, "InTransit")]
    [InlineData(false, false, false, "Available")]
    public void Compute_FollowsPrecedenceOrder(bool isRented, bool isInMaintenance, bool isInTransit, string expected)
    {
        var status = FleetStatusHelper.Compute("Available", isRented, isInMaintenance, isInTransit);
        Assert.Equal(expected, status);
    }

    [Fact]
    public void Compute_FallsBackToRawVehicleStatus_WhenNoOverridesApply()
    {
        var status = FleetStatusHelper.Compute("Accident", false, false, false);
        Assert.Equal("Accident", status);
    }

    private static async Task<(Vehicle Vehicle, Guid UserId)> SeedVehicleAsync(CarRentDbContext context)
    {
        var roleId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await context.Users.AddAsync(new User { Id = userId, FirstName = "Staff", LastName = "One", Email = $"{Guid.NewGuid():N}@example.com", RoleId = roleId });

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = $"REG-{Guid.NewGuid():N}",
            Vin = "VIN1",
            Year = 2024,
            FuelType = "Petrol",
            Transmission = "Automatic",
            SeatingCapacity = 5,
            DailyRate = 100,
            Status = "Available",
            BranchId = branchId
        };
        await context.Vehicles.AddAsync(vehicle);
        await context.SaveChangesAsync();

        return (vehicle, userId);
    }

    [Fact]
    public async Task AssignAsync_ClosesPreviousActiveAssignment_ForSameVehicle()
    {
        await using var context = CreateContext();
        var (vehicle, userId) = await SeedVehicleAsync(context);

        var driver1 = new Driver { Id = Guid.NewGuid(), UserId = userId, LicenseNumber = "LIC-1" };
        var user2Id = Guid.NewGuid();
        await context.Users.AddAsync(new User { Id = user2Id, FirstName = "Staff", LastName = "Two", Email = $"{Guid.NewGuid():N}@example.com", RoleId = Guid.NewGuid() });
        var driver2 = new Driver { Id = Guid.NewGuid(), UserId = user2Id, LicenseNumber = "LIC-2" };
        await context.Drivers.AddRangeAsync(driver1, driver2);
        await context.SaveChangesAsync();

        var service = new DriverAssignmentService(context);
        var assignerId = Guid.NewGuid();

        var first = await service.AssignAsync(new() { VehicleId = vehicle.Id, DriverId = driver1.Id }, assignerId);
        var second = await service.AssignAsync(new() { VehicleId = vehicle.Id, DriverId = driver2.Id }, assignerId);

        var history = await service.GetHistoryAsync(vehicle.Id, null);
        var firstEntry = history.First(a => a.Id == first.Id);
        var secondEntry = history.First(a => a.Id == second.Id);

        Assert.NotNull(firstEntry.UnassignedAt);
        Assert.Null(secondEntry.UnassignedAt);
    }

    [Fact]
    public async Task EndTripAsync_ComputesNonZeroDistance_ForMovingPings()
    {
        await using var context = CreateContext();
        var (vehicle, _) = await SeedVehicleAsync(context);

        var service = new FleetTrackingService(context);
        var trip = await service.StartTripAsync(new() { VehicleId = vehicle.Id, StartLatitude = 28.6139m, StartLongitude = 77.2090m });

        await service.RecordLocationAsync(new() { VehicleId = vehicle.Id, TripId = trip.Id, Latitude = 28.6200m, Longitude = 77.2150m });
        await service.RecordLocationAsync(new() { VehicleId = vehicle.Id, TripId = trip.Id, Latitude = 28.6300m, Longitude = 77.2250m });

        var ended = await service.EndTripAsync(trip.Id, new());

        Assert.Equal("Completed", ended.Status);
        Assert.True(ended.DistanceKm > 0);
    }

    [Fact]
    public async Task StartTripAsync_Throws_WhenVehicleAlreadyHasTripInProgress()
    {
        await using var context = CreateContext();
        var (vehicle, _) = await SeedVehicleAsync(context);

        var service = new FleetTrackingService(context);
        await service.StartTripAsync(new() { VehicleId = vehicle.Id, StartLatitude = 28.6139m, StartLongitude = 77.2090m });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartTripAsync(new() { VehicleId = vehicle.Id, StartLatitude = 28.6139m, StartLongitude = 77.2090m }));
    }
}
