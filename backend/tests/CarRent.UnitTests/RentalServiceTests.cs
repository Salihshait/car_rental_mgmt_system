using CarRent.Application.DTOs.Rentals;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CarRent.UnitTests;

public class RentalServiceTests
{
    private static CarRentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CarRentDbContext(options);
    }

    private static RentalService CreateService(CarRentDbContext context)
    {
        var settingsService = new SettingsService(context);
        var notificationService = new NotificationService(context);
        var emailService = new LoggingEmailService(NullLogger<LoggingEmailService>.Instance);
        var smsService = new LoggingSmsService(NullLogger<LoggingSmsService>.Instance);
        var invoiceService = new InvoiceService(context, new InvoicePdfService());
        var pdfService = new RentalAgreementPdfService();

        return new RentalService(context, settingsService, invoiceService, pdfService, notificationService, emailService, smsService);
    }

    private static async Task<(Booking Booking, Vehicle Vehicle, Guid CustomerId, Guid StaffId)> SeedConfirmedBookingAsync(CarRentDbContext context, decimal dailyRate = 100m, DateTime? endDate = null)
    {
        var customerId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        await context.Users.AddRangeAsync(
            new User { Id = customerId, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", RoleId = roleId },
            new User { Id = staffId, FirstName = "Staff", LastName = "Member", Email = "staff@example.com", RoleId = roleId });

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = $"REG-{Guid.NewGuid():N}",
            Vin = "VIN123",
            Year = 2024,
            FuelType = "Petrol",
            Transmission = "Automatic",
            SeatingCapacity = 5,
            DailyRate = dailyRate,
            Status = "Available",
            BranchId = branchId
        };
        await context.Vehicles.AddAsync(vehicle);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicle.Id,
            BranchId = branchId,
            BookingType = "Online",
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = endDate ?? DateTime.UtcNow,
            SubtotalAmount = dailyRate * 2,
            TotalAmount = dailyRate * 2,
            Status = "Confirmed"
        };
        await context.Bookings.AddAsync(booking);

        await context.SaveChangesAsync();

        return (booking, vehicle, customerId, staffId);
    }

    [Fact]
    public async Task ReturnAsync_NoLateFee_WhenReturnedBeforeEndDate()
    {
        await using var context = CreateContext();
        var (booking, _, _, staffId) = await SeedConfirmedBookingAsync(context, endDate: DateTime.UtcNow.AddHours(2));
        var service = CreateService(context);

        var rental = await service.PickupAsync(new CreatePickupRequest { BookingId = booking.Id, OdometerReading = 1000, FuelLevelPercent = 100 }, staffId);
        var result = await service.ReturnAsync(rental.Id, new CreateReturnRequest { ReturnAt = DateTime.UtcNow, OdometerReading = 1100, FuelLevelPercent = 90 }, staffId);

        Assert.Equal(0, result.LateFeeAmount);
        Assert.Equal(0, result.LateHours);
    }

    [Fact]
    public async Task ReturnAsync_ChargesLateFee_WhenReturnedAfterGracePeriod()
    {
        await using var context = CreateContext();
        var (booking, vehicle, _, staffId) = await SeedConfirmedBookingAsync(context, dailyRate: 100m, endDate: DateTime.UtcNow.AddHours(-3));
        var service = CreateService(context);

        var rental = await service.PickupAsync(new CreatePickupRequest { BookingId = booking.Id, OdometerReading = 1000, FuelLevelPercent = 100, SecurityDepositAmount = 0 }, staffId);

        // Booking.EndDate is 3 hours in the past; default grace is 60 minutes, so this is late by ~3 hours -> 1 late day.
        var result = await service.ReturnAsync(rental.Id, new CreateReturnRequest { ReturnAt = DateTime.UtcNow, OdometerReading = 1100, FuelLevelPercent = 90 }, staffId);

        var expectedLateFee = Math.Round(1 * vehicle.DailyRate * 1.5m, 2); // 1 late day * daily rate * default multiplier
        Assert.Equal(expectedLateFee, result.LateFeeAmount);
        Assert.True(result.LateHours > 0);

        var charges = await service.GetChargesAsync(rental.Id);
        Assert.Contains(charges, c => c.ChargeType == "Late" && c.Amount == expectedLateFee);
    }

    [Fact]
    public async Task ReturnAsync_FullyRefundsDeposit_WhenNoExtraCharges()
    {
        await using var context = CreateContext();
        var (booking, _, _, staffId) = await SeedConfirmedBookingAsync(context, endDate: DateTime.UtcNow.AddHours(2));
        var service = CreateService(context);

        var rental = await service.PickupAsync(new CreatePickupRequest { BookingId = booking.Id, OdometerReading = 1000, FuelLevelPercent = 100, SecurityDepositAmount = 150 }, staffId);
        var result = await service.ReturnAsync(rental.Id, new CreateReturnRequest { ReturnAt = DateTime.UtcNow, OdometerReading = 1100, FuelLevelPercent = 100 }, staffId);

        Assert.Equal("Refunded", result.SecurityDepositStatus);
        Assert.Equal(150, result.SecurityDepositRefundAmount);
    }

    [Fact]
    public async Task ReturnAsync_ForfeitsDeposit_WhenDamageChargesExceedDeposit()
    {
        await using var context = CreateContext();
        var (booking, _, _, staffId) = await SeedConfirmedBookingAsync(context, endDate: DateTime.UtcNow.AddHours(2));
        var service = CreateService(context);

        var rental = await service.PickupAsync(new CreatePickupRequest { BookingId = booking.Id, OdometerReading = 1000, FuelLevelPercent = 100, SecurityDepositAmount = 100 }, staffId);
        await service.AddDamageAsync(rental.Id, new CreateRentalDamageRequest { Stage = "Return", Description = "Cracked bumper", Severity = "Severe", EstimatedRepairCost = 300 }, staffId);

        var result = await service.ReturnAsync(rental.Id, new CreateReturnRequest { ReturnAt = DateTime.UtcNow, OdometerReading = 1100, FuelLevelPercent = 100 }, staffId);

        Assert.Equal("Forfeited", result.SecurityDepositStatus);
        Assert.Equal(0, result.SecurityDepositRefundAmount);

        var updatedBooking = await context.Bookings.FindAsync(booking.Id);
        Assert.Equal(booking.SubtotalAmount + 300, updatedBooking!.TotalAmount);
    }

    [Fact]
    public async Task ReturnAsync_PartiallyRefundsDeposit_WhenChargesAreLessThanDeposit()
    {
        await using var context = CreateContext();
        var (booking, _, _, staffId) = await SeedConfirmedBookingAsync(context, endDate: DateTime.UtcNow.AddHours(2));
        var service = CreateService(context);

        var rental = await service.PickupAsync(new CreatePickupRequest { BookingId = booking.Id, OdometerReading = 1000, FuelLevelPercent = 100, SecurityDepositAmount = 100 }, staffId);
        await service.AddDamageAsync(rental.Id, new CreateRentalDamageRequest { Stage = "Return", Description = "Scratch", Severity = "Minor", EstimatedRepairCost = 40 }, staffId);

        var result = await service.ReturnAsync(rental.Id, new CreateReturnRequest { ReturnAt = DateTime.UtcNow, OdometerReading = 1100, FuelLevelPercent = 100 }, staffId);

        Assert.Equal("PartiallyRefunded", result.SecurityDepositStatus);
        Assert.Equal(60, result.SecurityDepositRefundAmount);
    }

    [Fact]
    public async Task ReturnAsync_GeneratesInvoiceWithBaseAndExtraChargeLineItems()
    {
        await using var context = CreateContext();
        var (booking, _, _, staffId) = await SeedConfirmedBookingAsync(context, dailyRate: 100m, endDate: DateTime.UtcNow.AddHours(2));
        var service = CreateService(context);

        var rental = await service.PickupAsync(new CreatePickupRequest { BookingId = booking.Id, OdometerReading = 1000, FuelLevelPercent = 100, SecurityDepositAmount = 0 }, staffId);
        await service.AddDamageAsync(rental.Id, new CreateRentalDamageRequest { Stage = "Return", Description = "Scratch", Severity = "Minor", EstimatedRepairCost = 50 }, staffId);

        var result = await service.ReturnAsync(rental.Id, new CreateReturnRequest { ReturnAt = DateTime.UtcNow, OdometerReading = 1100, FuelLevelPercent = 100 }, staffId);

        Assert.NotNull(result.FinalInvoiceId);

        var lineItems = await context.InvoiceLineItems.Where(li => li.InvoiceId == result.FinalInvoiceId).ToListAsync();
        Assert.Equal(2, lineItems.Count);
        Assert.Contains(lineItems, li => li.Description == "Rental charge" && li.Amount == booking.SubtotalAmount);
        Assert.Contains(lineItems, li => li.Amount == 50);
    }
}
