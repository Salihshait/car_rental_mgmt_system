using System.Globalization;
using CarRent.Application.DTOs.Vehicles;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    public static readonly string[] ValidStatuses =
    {
        "Available", "Booked", "Maintenance", "Cleaning", "On Rent", "Reserved", "Accident"
    };

    private readonly CarRentDbContext _context;

    public VehicleService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleDto>> GetAllAsync(VehicleFilter filter, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(Query(), filter);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<VehicleDto> CreateAsync(SaveVehicleRequest request, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAsync(request, cancellationToken);

        if (await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == request.RegistrationNumber, cancellationToken))
        {
            throw new InvalidOperationException("A vehicle with this registration number already exists.");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = request.RegistrationNumber,
            Vin = request.Vin,
            EngineNumber = request.EngineNumber,
            Color = request.Color,
            Year = request.Year,
            FuelType = request.FuelType,
            Transmission = request.Transmission,
            SeatingCapacity = request.SeatingCapacity,
            DailyRate = request.DailyRate,
            GpsDeviceId = request.GpsDeviceId,
            BrandId = request.BrandId,
            ModelId = request.ModelId,
            BranchId = request.BranchId,
            Status = "Available"
        };

        await _context.Vehicles.AddAsync(vehicle, cancellationToken);
        await WriteAuditAsync(vehicle.Id, "Created", $"Vehicle {vehicle.RegistrationNumber} created.", actingUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(vehicle.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created vehicle.");
    }

    public async Task<VehicleDto> UpdateAsync(Guid id, SaveVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        await ValidateReferencesAsync(request, cancellationToken);

        var registrationInUse = await _context.Vehicles
            .AnyAsync(v => v.Id != id && v.RegistrationNumber == request.RegistrationNumber, cancellationToken);
        if (registrationInUse)
        {
            throw new InvalidOperationException("Another vehicle already uses this registration number.");
        }

        vehicle.RegistrationNumber = request.RegistrationNumber;
        vehicle.Vin = request.Vin;
        vehicle.EngineNumber = request.EngineNumber;
        vehicle.Color = request.Color;
        vehicle.Year = request.Year;
        vehicle.FuelType = request.FuelType;
        vehicle.Transmission = request.Transmission;
        vehicle.SeatingCapacity = request.SeatingCapacity;
        vehicle.DailyRate = request.DailyRate;
        vehicle.GpsDeviceId = request.GpsDeviceId;
        vehicle.BrandId = request.BrandId;
        vehicle.ModelId = request.ModelId;
        vehicle.BranchId = request.BranchId;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated vehicle.");
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FindAsync(new object?[] { id }, cancellationToken);
        if (vehicle is null)
        {
            return false;
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<VehicleDto> UpdateStatusAsync(Guid id, string status, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        if (!ValidStatuses.Contains(status))
        {
            throw new InvalidOperationException($"'{status}' is not a valid vehicle status.");
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var previousStatus = vehicle.Status;
        vehicle.Status = status;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await WriteAuditAsync(id, "StatusChanged", $"Status changed from {previousStatus} to {status}.", actingUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated vehicle.");
    }

    public async Task<IEnumerable<VehicleTimelineEntryDto>> GetTimelineAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == "Vehicle" && a.EntityId == vehicleId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new VehicleTimelineEntryDto
            {
                Id = a.Id,
                Action = a.Action,
                Payload = a.Payload,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleImportResultDto> ImportAsync(Stream csvStream, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);

        var result = new VehicleImportResultDto();
        var rowNumber = 1;

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            rowNumber++;
            result.TotalRows++;
            var registrationNumber = csv.GetField("RegistrationNumber") ?? string.Empty;

            try
            {
                var brandName = csv.GetField("Brand");
                var modelName = csv.GetField("Model");
                var branchName = csv.GetField("Branch");

                var brand = string.IsNullOrWhiteSpace(brandName) ? null
                    : await _context.Brands.FirstOrDefaultAsync(b => b.Name == brandName, cancellationToken)
                        ?? throw new InvalidOperationException($"Brand '{brandName}' not found.");

                var model = string.IsNullOrWhiteSpace(modelName) ? null
                    : await _context.Models.FirstOrDefaultAsync(m => m.Name == modelName, cancellationToken)
                        ?? throw new InvalidOperationException($"Model '{modelName}' not found.");

                var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Name == branchName, cancellationToken)
                    ?? throw new InvalidOperationException($"Branch '{branchName}' not found.");

                if (string.IsNullOrWhiteSpace(registrationNumber))
                {
                    throw new InvalidOperationException("RegistrationNumber is required.");
                }

                if (await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == registrationNumber, cancellationToken))
                {
                    throw new InvalidOperationException("A vehicle with this registration number already exists.");
                }

                var vehicle = new Vehicle
                {
                    Id = Guid.NewGuid(),
                    RegistrationNumber = registrationNumber,
                    Vin = csv.GetField("Vin") ?? string.Empty,
                    EngineNumber = csv.GetField("EngineNumber"),
                    Color = csv.GetField("Color"),
                    Year = int.TryParse(csv.GetField("Year"), out var year) ? year : 0,
                    FuelType = csv.GetField("FuelType") ?? string.Empty,
                    Transmission = csv.GetField("Transmission") ?? string.Empty,
                    SeatingCapacity = int.TryParse(csv.GetField("SeatingCapacity"), out var seats) ? seats : 0,
                    DailyRate = decimal.TryParse(csv.GetField("DailyRate"), out var rate) ? rate : 0,
                    GpsDeviceId = csv.GetField("GpsDeviceId"),
                    BrandId = brand?.Id,
                    ModelId = model?.Id,
                    BranchId = branch.Id,
                    Status = "Available"
                };

                await _context.Vehicles.AddAsync(vehicle, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await WriteAuditAsync(vehicle.Id, "Imported", $"Vehicle {vehicle.RegistrationNumber} imported via CSV.", actingUserId, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                result.SuccessCount++;
                result.Rows.Add(new VehicleImportRowResultDto { RowNumber = rowNumber, Success = true, RegistrationNumber = registrationNumber });
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Rows.Add(new VehicleImportRowResultDto
                {
                    RowNumber = rowNumber,
                    Success = false,
                    Message = ex.Message,
                    RegistrationNumber = registrationNumber
                });
            }
        }

        return result;
    }

    public async Task<byte[]> ExportAsync(VehicleFilter filter, CancellationToken cancellationToken = default)
    {
        var vehicles = await ApplyFilter(Query(), filter).ToListAsync(cancellationToken);

        using var memoryStream = new MemoryStream();
        await using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteField("RegistrationNumber");
            csv.WriteField("Vin");
            csv.WriteField("EngineNumber");
            csv.WriteField("Color");
            csv.WriteField("Year");
            csv.WriteField("FuelType");
            csv.WriteField("Transmission");
            csv.WriteField("SeatingCapacity");
            csv.WriteField("DailyRate");
            csv.WriteField("Status");
            csv.WriteField("GpsDeviceId");
            csv.WriteField("Brand");
            csv.WriteField("Model");
            csv.WriteField("Branch");
            await csv.NextRecordAsync();

            foreach (var vehicle in vehicles)
            {
                csv.WriteField(vehicle.RegistrationNumber);
                csv.WriteField(vehicle.Vin);
                csv.WriteField(vehicle.EngineNumber);
                csv.WriteField(vehicle.Color);
                csv.WriteField(vehicle.Year);
                csv.WriteField(vehicle.FuelType);
                csv.WriteField(vehicle.Transmission);
                csv.WriteField(vehicle.SeatingCapacity);
                csv.WriteField(vehicle.DailyRate);
                csv.WriteField(vehicle.Status);
                csv.WriteField(vehicle.GpsDeviceId);
                csv.WriteField(vehicle.BrandName);
                csv.WriteField(vehicle.ModelName);
                csv.WriteField(vehicle.BranchName);
                await csv.NextRecordAsync();
            }
        }

        return memoryStream.ToArray();
    }

    public async Task<FleetDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var statusCounts = await _context.Vehicles
            .AsNoTracking()
            .GroupBy(v => v.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var total = statusCounts.Sum(s => s.Count);
        var available = statusCounts.FirstOrDefault(s => s.Status == "Available")?.Count ?? 0;

        var revenue = await _context.Invoices
            .AsNoTracking()
            .SumAsync(i => (decimal?)i.TotalAmount, cancellationToken) ?? 0;

        return new FleetDashboardDto
        {
            TotalVehicles = total,
            AvailableVehicles = available,
            UtilizationPercent = total == 0 ? 0 : Math.Round((double)(total - available) / total * 100, 1),
            Revenue = revenue,
            StatusCounts = statusCounts.ToDictionary(s => s.Status, s => s.Count)
        };
    }

    private async Task ValidateReferencesAsync(SaveVehicleRequest request, CancellationToken cancellationToken)
    {
        if (!await _context.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new InvalidOperationException("The selected branch does not exist.");
        }

        if (request.BrandId.HasValue && !await _context.Brands.AnyAsync(b => b.Id == request.BrandId, cancellationToken))
        {
            throw new InvalidOperationException("The selected brand does not exist.");
        }

        if (request.ModelId.HasValue && !await _context.Models.AnyAsync(m => m.Id == request.ModelId, cancellationToken))
        {
            throw new InvalidOperationException("The selected model does not exist.");
        }
    }

    private async Task WriteAuditAsync(Guid vehicleId, string action, string message, Guid? actingUserId, CancellationToken cancellationToken)
    {
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = actingUserId,
            Action = action,
            EntityType = "Vehicle",
            EntityId = vehicleId,
            Payload = $"{{\"message\":\"{message.Replace("\"", "'")}\"}}"
        }, cancellationToken);
    }

    private static IQueryable<VehicleDto> ApplyFilter(IQueryable<VehicleDto> query, VehicleFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(v =>
                v.RegistrationNumber.Contains(term) ||
                v.Vin.Contains(term) ||
                (v.EngineNumber != null && v.EngineNumber.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(v => v.Status == filter.Status);
        }

        if (filter.BrandId.HasValue)
        {
            query = query.Where(v => v.BrandId == filter.BrandId);
        }

        if (filter.BranchId.HasValue)
        {
            query = query.Where(v => v.BranchId == filter.BranchId);
        }

        if (!string.IsNullOrWhiteSpace(filter.FuelType))
        {
            query = query.Where(v => v.FuelType == filter.FuelType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Transmission))
        {
            query = query.Where(v => v.Transmission == filter.Transmission);
        }

        return query;
    }

    private IQueryable<VehicleDto> Query()
    {
        var query = _context.Vehicles
            .AsNoTracking()
            .Include(v => v.Brand)
            .Include(v => v.VehicleModel).ThenInclude(m => m!.Category)
            .Include(v => v.Branch)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Vin = v.Vin,
                EngineNumber = v.EngineNumber,
                Color = v.Color,
                Year = v.Year,
                FuelType = v.FuelType,
                Transmission = v.Transmission,
                SeatingCapacity = v.SeatingCapacity,
                DailyRate = v.DailyRate,
                Status = v.Status,
                GpsDeviceId = v.GpsDeviceId,
                BrandId = v.BrandId,
                BrandName = v.Brand != null ? v.Brand.Name : null,
                ModelId = v.ModelId,
                ModelName = v.VehicleModel != null ? v.VehicleModel.Name : null,
                CategoryName = v.VehicleModel != null ? v.VehicleModel.Category.Name : null,
                BranchId = v.BranchId,
                BranchName = v.Branch.Name,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            });

        return query;
    }
}
