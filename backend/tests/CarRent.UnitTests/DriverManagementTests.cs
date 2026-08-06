using CarRent.Application.DTOs.Drivers;
using CarRent.Application.DTOs.Fleet;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRent.UnitTests;

public class DriverManagementTests
{
    private static CarRentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CarRentDbContext(options);
    }

    private static async Task<Driver> SeedDriverAsync(CarRentDbContext context)
    {
        var userId = Guid.NewGuid();
        await context.Users.AddAsync(new User { Id = userId, FirstName = "Dan", LastName = "Driver", Email = $"{Guid.NewGuid():N}@example.com", RoleId = Guid.NewGuid() });

        var driver = new Driver { Id = Guid.NewGuid(), UserId = userId, LicenseNumber = $"LIC-{Guid.NewGuid():N}" };
        await context.Drivers.AddAsync(driver);
        await context.SaveChangesAsync();

        return driver;
    }

    [Fact]
    public async Task GenerateAsync_ComputesNetAmount_FromBaseDeductionsAndBonus()
    {
        await using var context = CreateContext();
        var driver = await SeedDriverAsync(context);
        var service = new DriverSalaryService(context);

        var result = await service.GenerateAsync(new CreateSalaryPaymentRequest
        {
            DriverId = driver.Id,
            PeriodStart = new DateTime(2026, 1, 1),
            PeriodEnd = new DateTime(2026, 1, 31),
            BaseAmount = 2000,
            Deductions = 150,
            Bonus = 100
        }, Guid.NewGuid());

        Assert.Equal(1950, result.NetAmount);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task GenerateAsync_Throws_WhenNetAmountWouldBeNegative()
    {
        await using var context = CreateContext();
        var driver = await SeedDriverAsync(context);
        var service = new DriverSalaryService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateAsync(new CreateSalaryPaymentRequest
        {
            DriverId = driver.Id,
            PeriodStart = new DateTime(2026, 1, 1),
            PeriodEnd = new DateTime(2026, 1, 31),
            BaseAmount = 100,
            Deductions = 500,
            Bonus = 0
        }, Guid.NewGuid()));
    }

    [Fact]
    public async Task AddAsync_RecomputesDriverAverageRating()
    {
        await using var context = CreateContext();
        var driver = await SeedDriverAsync(context);
        var service = new DriverRatingService(context);

        await service.AddAsync(new CreateDriverRatingRequest { DriverId = driver.Id, Score = 4 }, Guid.NewGuid());
        await service.AddAsync(new CreateDriverRatingRequest { DriverId = driver.Id, Score = 2 }, Guid.NewGuid());

        var updated = await context.Drivers.AsNoTracking().FirstAsync(d => d.Id == driver.Id);
        Assert.Equal(3, updated.Rating);
    }

    [Fact]
    public async Task CheckInThenCheckOut_Succeeds()
    {
        await using var context = CreateContext();
        var driver = await SeedDriverAsync(context);
        var service = new DriverAttendanceService(context);

        var checkedIn = await service.CheckInAsync(driver.Id);
        Assert.NotNull(checkedIn.CheckInAt);
        Assert.Equal("Present", checkedIn.Status);

        var checkedOut = await service.CheckOutAsync(driver.Id);
        Assert.NotNull(checkedOut.CheckOutAt);
    }

    [Fact]
    public async Task CheckOutAsync_Throws_WhenNotCheckedInYet()
    {
        await using var context = CreateContext();
        var driver = await SeedDriverAsync(context);
        var service = new DriverAttendanceService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckOutAsync(driver.Id));
    }

    [Fact]
    public async Task CheckInAsync_Throws_OnSecondCheckInSameDay()
    {
        await using var context = CreateContext();
        var driver = await SeedDriverAsync(context);
        var service = new DriverAttendanceService(context);

        await service.CheckInAsync(driver.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckInAsync(driver.Id));
    }

    [Theory]
    [InlineData(null, "Valid")]
    [InlineData(400, "Valid")]
    [InlineData(15, "ExpiringSoon")]
    [InlineData(-5, "Expired")]
    public async Task GetByIdAsync_ComputesLicenseStatus_FromExpiryDate(int? daysFromNow, string expectedStatus)
    {
        await using var context = CreateContext();
        var driver = await SeedDriverAsync(context);
        var trackingService = new FleetTrackingService(context);
        var driverService = new DriverService(context, trackingService);

        await driverService.UpdateAsync(driver.Id, new UpdateDriverRequest
        {
            LicenseNumber = driver.LicenseNumber,
            KycStatus = "Verified",
            EmploymentStatus = "Active",
            LicenseExpiryDate = daysFromNow.HasValue ? DateTime.UtcNow.AddDays(daysFromNow.Value) : null
        });

        var result = await driverService.GetByIdAsync(driver.Id);

        Assert.Equal(expectedStatus, result!.LicenseStatus);
    }
}
