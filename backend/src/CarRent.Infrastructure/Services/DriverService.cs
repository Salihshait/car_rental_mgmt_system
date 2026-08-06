using CarRent.Application.DTOs.Drivers;
using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class DriverService : IDriverService
{
    private readonly CarRentDbContext _context;
    private readonly IFleetTrackingService _trackingService;

    public DriverService(CarRentDbContext context, IFleetTrackingService trackingService)
    {
        _context = context;
        _trackingService = trackingService;
    }

    public async Task<IEnumerable<DriverDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await _context.Drivers.AsNoTracking().ToListAsync(cancellationToken);
        return await MapAsync(drivers, cancellationToken);
    }

    public async Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (driver is null)
        {
            return null;
        }

        var mapped = await MapAsync(new List<Driver> { driver }, cancellationToken);
        return mapped.First();
    }

    public async Task<DriverDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
        if (driver is null)
        {
            return null;
        }

        var mapped = await MapAsync(new List<Driver> { driver }, cancellationToken);
        return mapped.First();
    }

    public async Task<DriverDto> CreateAsync(CreateDriverRequest request, CancellationToken cancellationToken = default)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (await _context.Drivers.AnyAsync(d => d.UserId == request.UserId, cancellationToken))
        {
            throw new InvalidOperationException("This user is already registered as a driver.");
        }

        if (await _context.Drivers.AnyAsync(d => d.LicenseNumber == request.LicenseNumber, cancellationToken))
        {
            throw new InvalidOperationException("A driver with this license number already exists.");
        }

        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            LicenseNumber = request.LicenseNumber
        };

        await _context.Drivers.AddAsync(driver, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(driver.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created driver.");
    }

    public async Task<DriverDto> UpdateAsync(Guid id, UpdateDriverRequest request, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Driver not found.");

        if (await _context.Drivers.AnyAsync(d => d.Id != id && d.LicenseNumber == request.LicenseNumber, cancellationToken))
        {
            throw new InvalidOperationException("Another driver already uses this license number.");
        }

        if (request.DepartmentId.HasValue && !await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId, cancellationToken))
        {
            throw new InvalidOperationException("Department not found.");
        }

        if (request.BranchId.HasValue && !await _context.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new InvalidOperationException("Branch not found.");
        }

        driver.LicenseNumber = request.LicenseNumber;
        driver.KycStatus = request.KycStatus;
        driver.PhotoUrl = request.PhotoUrl;
        driver.Address = request.Address;
        driver.EmergencyContactName = request.EmergencyContactName;
        driver.EmergencyContactPhone = request.EmergencyContactPhone;
        driver.DateOfJoining = request.DateOfJoining;
        driver.EmploymentStatus = request.EmploymentStatus;
        driver.DepartmentId = request.DepartmentId;
        driver.BranchId = request.BranchId;
        driver.LicenseExpiryDate = request.LicenseExpiryDate;
        driver.BaseSalary = request.BaseSalary;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated driver.");
    }

    public async Task<DriverDto> SelfUpdateAsync(Guid userId, SelfUpdateDriverRequest request, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Driver record not found for this account.");

        driver.PhotoUrl = request.PhotoUrl;
        driver.Address = request.Address;
        driver.EmergencyContactName = request.EmergencyContactName;
        driver.EmergencyContactPhone = request.EmergencyContactPhone;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(driver.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated driver.");
    }

    public async Task<DriverPerformanceSummaryDto> GetPerformanceSummaryAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == driverId, cancellationToken)
            ?? throw new InvalidOperationException("Driver not found.");

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var trips = await _trackingService.GetTripsAsync(new TripFilter { DriverId = driverId, DateFrom = monthStart }, cancellationToken);
        var tripList = trips.ToList();

        var attendanceThisMonth = await _context.DriverAttendances
            .AsNoTracking()
            .Where(a => a.DriverId == driverId && a.AttendanceDate >= monthStart)
            .ToListAsync(cancellationToken);
        var presentDays = attendanceThisMonth.Count(a => a.Status == "Present");
        var attendanceRate = attendanceThisMonth.Count == 0 ? 0 : Math.Round((double)presentDays / attendanceThisMonth.Count * 100, 1);

        var ratingScores = await _context.DriverRatings.AsNoTracking().Where(r => r.DriverId == driverId).Select(r => r.Score).ToListAsync(cancellationToken);

        var activeAssignment = await _context.DriverAssignments.AsNoTracking().FirstOrDefaultAsync(a => a.DriverId == driverId && a.UnassignedAt == null, cancellationToken);
        var vehicle = activeAssignment is null ? null : await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == activeAssignment.VehicleId, cancellationToken);

        return new DriverPerformanceSummaryDto
        {
            DriverId = driverId,
            TripsThisMonth = tripList.Count,
            DistanceThisMonthKm = tripList.Sum(t => t.DistanceKm),
            AttendanceRateThisMonth = attendanceRate,
            AverageRating = driver.Rating,
            RatingCount = ratingScores.Count,
            CurrentVehicleRegistrationNumber = vehicle?.RegistrationNumber
        };
    }

    public async Task<DriverManagementDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await _context.Drivers.AsNoTracking().ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var today = now.Date;
        var expiringThreshold = now.AddDays(30);

        var onLeaveToday = await _context.DriverAttendances.AsNoTracking()
            .CountAsync(a => a.AttendanceDate == today && a.Status == "Leave", cancellationToken);

        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var driverIds = drivers.Select(d => d.Id).ToList();
        var tripsThisMonth = await _context.Trips.AsNoTracking()
            .CountAsync(t => t.DriverId.HasValue && driverIds.Contains(t.DriverId.Value) && t.StartedAt >= monthStart, cancellationToken);

        var ratedDrivers = drivers.Where(d => d.Rating.HasValue).ToList();

        return new DriverManagementDashboardDto
        {
            TotalDrivers = drivers.Count,
            ActiveDrivers = drivers.Count(d => d.EmploymentStatus == "Active"),
            OnLeaveToday = onLeaveToday,
            LicensesExpiringSoonCount = drivers.Count(d => d.LicenseExpiryDate.HasValue && d.LicenseExpiryDate.Value >= now && d.LicenseExpiryDate.Value <= expiringThreshold),
            LicensesExpiredCount = drivers.Count(d => d.LicenseExpiryDate.HasValue && d.LicenseExpiryDate.Value < now),
            AverageRating = ratedDrivers.Count == 0 ? null : Math.Round(ratedDrivers.Average(d => d.Rating!.Value), 2),
            TotalTripsThisMonth = tripsThisMonth
        };
    }

    private async Task<List<DriverDto>> MapAsync(List<Driver> drivers, CancellationToken cancellationToken)
    {
        var userIds = drivers.Select(d => d.UserId).ToList();
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);

        var driverIds = drivers.Select(d => d.Id).ToList();
        var activeAssignments = await _context.DriverAssignments
            .AsNoTracking()
            .Where(a => driverIds.Contains(a.DriverId) && a.UnassignedAt == null)
            .ToListAsync(cancellationToken);

        var vehicleIds = activeAssignments.Select(a => a.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var departmentIds = drivers.Where(d => d.DepartmentId.HasValue).Select(d => d.DepartmentId!.Value).Distinct().ToList();
        var departments = await _context.Departments.AsNoTracking().Where(d => departmentIds.Contains(d.Id)).ToListAsync(cancellationToken);

        var branchIds = drivers.Where(d => d.BranchId.HasValue).Select(d => d.BranchId!.Value).Distinct().ToList();
        var branches = await _context.Branches.AsNoTracking().Where(b => branchIds.Contains(b.Id)).ToListAsync(cancellationToken);

        return drivers.Select(d =>
        {
            var user = users.FirstOrDefault(u => u.Id == d.UserId);
            var assignment = activeAssignments.FirstOrDefault(a => a.DriverId == d.Id);
            var vehicle = assignment is null ? null : vehicles.FirstOrDefault(v => v.Id == assignment.VehicleId);
            var department = d.DepartmentId.HasValue ? departments.FirstOrDefault(dep => dep.Id == d.DepartmentId) : null;
            var branch = d.BranchId.HasValue ? branches.FirstOrDefault(b => b.Id == d.BranchId) : null;

            return new DriverDto
            {
                Id = d.Id,
                UserId = d.UserId,
                Name = user is null ? null : $"{user.FirstName} {user.LastName}",
                Email = user?.Email,
                PhoneNumber = user?.PhoneNumber,
                LicenseNumber = d.LicenseNumber,
                KycStatus = d.KycStatus,
                Rating = d.Rating,
                CurrentVehicleId = vehicle?.Id,
                CurrentVehicleRegistrationNumber = vehicle?.RegistrationNumber,
                PhotoUrl = d.PhotoUrl,
                Address = d.Address,
                EmergencyContactName = d.EmergencyContactName,
                EmergencyContactPhone = d.EmergencyContactPhone,
                DateOfJoining = d.DateOfJoining,
                EmploymentStatus = d.EmploymentStatus,
                DepartmentId = d.DepartmentId,
                DepartmentName = department?.Name,
                BranchId = d.BranchId,
                BranchName = branch?.Name,
                LicenseExpiryDate = d.LicenseExpiryDate,
                LicenseStatus = ComputeLicenseStatus(d.LicenseExpiryDate),
                BaseSalary = d.BaseSalary,
                CreatedAt = d.CreatedAt
            };
        }).ToList();
    }

    private static string ComputeLicenseStatus(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue)
        {
            return "Valid";
        }

        var now = DateTime.UtcNow;
        if (expiryDate.Value < now)
        {
            return "Expired";
        }

        return expiryDate.Value <= now.AddDays(30) ? "ExpiringSoon" : "Valid";
    }
}
