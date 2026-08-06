using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRent.UnitTests;

public class AvailabilityServiceTests
{
    private static CarRentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CarRentDbContext(options);
    }

    private static Booking MakeBooking(Guid vehicleId, DateTime start, DateTime end, string status = "Confirmed") => new()
    {
        Id = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        VehicleId = vehicleId,
        StartDate = start,
        EndDate = end,
        Status = status,
        BookingType = "Online"
    };

    [Fact]
    public async Task IsVehicleAvailableAsync_ReturnsTrue_WhenNoBookingsExist()
    {
        await using var context = CreateContext();
        var service = new AvailabilityService(context);
        var vehicleId = Guid.NewGuid();

        var result = await service.IsVehicleAvailableAsync(vehicleId, new DateTime(2026, 1, 1), new DateTime(2026, 1, 5));

        Assert.True(result);
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_ReturnsFalse_WhenDatesOverlapConfirmedBooking()
    {
        await using var context = CreateContext();
        var vehicleId = Guid.NewGuid();
        await context.Bookings.AddAsync(MakeBooking(vehicleId, new DateTime(2026, 1, 3), new DateTime(2026, 1, 10)));
        await context.SaveChangesAsync();

        var service = new AvailabilityService(context);
        var result = await service.IsVehicleAvailableAsync(vehicleId, new DateTime(2026, 1, 1), new DateTime(2026, 1, 5));

        Assert.False(result);
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_ReturnsTrue_WhenOverlappingBookingIsCancelled()
    {
        await using var context = CreateContext();
        var vehicleId = Guid.NewGuid();
        await context.Bookings.AddAsync(MakeBooking(vehicleId, new DateTime(2026, 1, 3), new DateTime(2026, 1, 10), status: "Cancelled"));
        await context.SaveChangesAsync();

        var service = new AvailabilityService(context);
        var result = await service.IsVehicleAvailableAsync(vehicleId, new DateTime(2026, 1, 1), new DateTime(2026, 1, 5));

        Assert.True(result);
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_ReturnsTrue_WhenRangesAreOnlyAdjacent()
    {
        await using var context = CreateContext();
        var vehicleId = Guid.NewGuid();
        // Existing booking ends exactly when the new one starts - should not count as overlap.
        await context.Bookings.AddAsync(MakeBooking(vehicleId, new DateTime(2026, 1, 1), new DateTime(2026, 1, 5)));
        await context.SaveChangesAsync();

        var service = new AvailabilityService(context);
        var result = await service.IsVehicleAvailableAsync(vehicleId, new DateTime(2026, 1, 5), new DateTime(2026, 1, 10));

        Assert.True(result);
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_ExcludesGivenBookingId_ForModifyScenario()
    {
        await using var context = CreateContext();
        var vehicleId = Guid.NewGuid();
        var existing = MakeBooking(vehicleId, new DateTime(2026, 1, 1), new DateTime(2026, 1, 10));
        await context.Bookings.AddAsync(existing);
        await context.SaveChangesAsync();

        var service = new AvailabilityService(context);

        // Modifying the same booking to slightly different dates should not conflict with itself.
        var resultExcludingSelf = await service.IsVehicleAvailableAsync(vehicleId, new DateTime(2026, 1, 2), new DateTime(2026, 1, 9), excludeBookingId: existing.Id);
        Assert.True(resultExcludingSelf);

        // Without excluding it, the same range should be blocked by its own row.
        var resultWithoutExclusion = await service.IsVehicleAvailableAsync(vehicleId, new DateTime(2026, 1, 2), new DateTime(2026, 1, 9));
        Assert.False(resultWithoutExclusion);
    }
}
