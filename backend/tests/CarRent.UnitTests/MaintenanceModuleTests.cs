using CarRent.Application.DTOs.Fleet;
using CarRent.Application.DTOs.Maintenance;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRent.UnitTests;

public class MaintenanceModuleTests
{
    private static CarRentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CarRentDbContext(options);
    }

    private static async Task<Vehicle> SeedVehicleAsync(CarRentDbContext context)
    {
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
            BranchId = Guid.NewGuid()
        };
        await context.Vehicles.AddAsync(vehicle);
        await context.SaveChangesAsync();
        return vehicle;
    }

    [Fact]
    public async Task RecordUsageAsync_DeductsStock_AndComputesTotal()
    {
        await using var context = CreateContext();
        var part = new SparePart { Id = Guid.NewGuid(), PartNumber = "P-1", Name = "Brake Pad", UnitCost = 25, StockQuantity = 10, ReorderLevel = 2 };
        await context.SpareParts.AddAsync(part);
        var vehicle = await SeedVehicleAsync(context);
        var maintenance = new VehicleMaintenance { Id = Guid.NewGuid(), VehicleId = vehicle.Id, ServiceType = "Brake Service", ScheduledOn = DateTime.UtcNow, Status = "InProgress" };
        await context.VehicleMaintenances.AddAsync(maintenance);
        await context.SaveChangesAsync();

        var service = new SparePartService(context);
        var usage = await service.RecordUsageAsync(maintenance.Id, new RecordPartUsageRequest { SparePartId = part.Id, Quantity = 3 });

        Assert.Equal(75, usage.TotalAmount);

        var updatedPart = await context.SpareParts.AsNoTracking().FirstAsync(p => p.Id == part.Id);
        Assert.Equal(7, updatedPart.StockQuantity);
    }

    [Fact]
    public async Task RecordUsageAsync_Throws_WhenStockInsufficient()
    {
        await using var context = CreateContext();
        var part = new SparePart { Id = Guid.NewGuid(), PartNumber = "P-2", Name = "Filter", UnitCost = 10, StockQuantity = 2, ReorderLevel = 1 };
        await context.SpareParts.AddAsync(part);
        var vehicle = await SeedVehicleAsync(context);
        var maintenance = new VehicleMaintenance { Id = Guid.NewGuid(), VehicleId = vehicle.Id, ServiceType = "Service", ScheduledOn = DateTime.UtcNow, Status = "InProgress" };
        await context.VehicleMaintenances.AddAsync(maintenance);
        await context.SaveChangesAsync();

        var service = new SparePartService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RecordUsageAsync(maintenance.Id, new RecordPartUsageRequest { SparePartId = part.Id, Quantity = 5 }));
    }

    [Theory]
    [InlineData(400, "Active")]
    [InlineData(15, "ExpiringSoon")]
    [InlineData(-5, "Expired")]
    public void ComputeExpiryStatus_ReturnsExpectedStatus_FromDaysOffset(int daysFromNow, string expected)
    {
        var status = MaintenanceStatusHelper.ComputeExpiryStatus("Active", DateTime.UtcNow.AddDays(daysFromNow));
        Assert.Equal(expected, status);
    }

    [Fact]
    public void ComputeExpiryStatus_HonorsManualCancelledOverride()
    {
        var status = MaintenanceStatusHelper.ComputeExpiryStatus("Cancelled", DateTime.UtcNow.AddDays(400));
        Assert.Equal("Cancelled", status);
    }

    [Fact]
    public async Task GetCostSummaryAsync_ReconcilesMaintenancePartsAndExpenses()
    {
        await using var context = CreateContext();
        var vehicle = await SeedVehicleAsync(context);
        var userId = Guid.NewGuid();
        await context.Users.AddAsync(new User { Id = userId, FirstName = "Ops", LastName = "Staff", Email = $"{Guid.NewGuid():N}@example.com", RoleId = Guid.NewGuid() });

        var maintenance = new VehicleMaintenance
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            ServiceType = "Oil Change",
            ScheduledOn = DateTime.UtcNow,
            Status = "Completed",
            Cost = 100
        };
        await context.VehicleMaintenances.AddAsync(maintenance);

        var part = new SparePart { Id = Guid.NewGuid(), PartNumber = "P-3", Name = "Oil Filter", UnitCost = 20, StockQuantity = 10, ReorderLevel = 1 };
        await context.SpareParts.AddAsync(part);
        await context.MaintenancePartUsages.AddAsync(new MaintenancePartUsage { Id = Guid.NewGuid(), MaintenanceId = maintenance.Id, SparePartId = part.Id, Quantity = 2, UnitCost = 20, TotalAmount = 40 });

        await context.MaintenanceExpenses.AddAsync(new MaintenanceExpense { Id = Guid.NewGuid(), VehicleId = vehicle.Id, Category = "Towing", Amount = 30, CreatedBy = userId, ExpenseDate = DateTime.UtcNow });

        await context.SaveChangesAsync();

        var reportService = new MaintenanceReportService(context);
        var summary = await reportService.GetCostSummaryAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null, null, null);

        Assert.Equal(100, summary.MaintenanceCost);
        Assert.Equal(40, summary.PartsCost);
        Assert.Equal(30, summary.ExpensesCost);
        Assert.Equal(170, summary.TotalCost);
        Assert.Equal(170, summary.CostByVehicle.Single(v => v.VehicleId == vehicle.Id).TotalCost);
    }

    [Fact]
    public async Task GetCalendarAsync_ReturnsEntriesFromAllFourSources()
    {
        await using var context = CreateContext();
        var vehicle = await SeedVehicleAsync(context);
        var vendor = new Vendor { Id = Guid.NewGuid(), Name = "Acme Motors" };
        await context.Vendors.AddAsync(vendor);

        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(20);

        await context.VehicleMaintenances.AddAsync(new VehicleMaintenance { Id = Guid.NewGuid(), VehicleId = vehicle.Id, ServiceType = "Tire Rotation", ScheduledOn = from.AddDays(5), Status = "Scheduled" });
        await context.AmcContracts.AddAsync(new AmcContract { Id = Guid.NewGuid(), VehicleId = vehicle.Id, VendorId = vendor.Id, ContractNumber = "AMC-1", StartDate = from.AddDays(-300), EndDate = from.AddDays(10), Cost = 500 });
        await context.VehicleWarranties.AddAsync(new VehicleWarranty { Id = Guid.NewGuid(), VehicleId = vehicle.Id, WarrantyType = "Manufacturer", StartDate = from.AddDays(-300), EndDate = from.AddDays(15) });
        await context.VehicleInspections.AddAsync(new VehicleInspection { Id = Guid.NewGuid(), VehicleId = vehicle.Id, InspectionType = "Safety", InspectionDate = from.AddDays(-10), NextDueDate = from.AddDays(8) });
        await context.SaveChangesAsync();

        var reportService = new MaintenanceReportService(context);
        var entries = (await reportService.GetCalendarAsync(from, to)).ToList();

        Assert.Contains(entries, e => e.Type == "Maintenance");
        Assert.Contains(entries, e => e.Type == "AMC");
        Assert.Contains(entries, e => e.Type == "Warranty");
        Assert.Contains(entries, e => e.Type == "Inspection");
        Assert.Equal(4, entries.Count);
    }
}
