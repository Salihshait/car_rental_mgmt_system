using CarRent.Application.DTOs.Vehicles;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private readonly CarRentDbContext _context;

    public VehicleService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleListDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Select(v => new VehicleListDto
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                DailyRate = v.DailyRate,
                FuelType = v.FuelType,
                Transmission = v.Transmission,
                SeatingCapacity = v.SeatingCapacity,
                IsAvailable = v.IsAvailable
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleListDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VehicleListDto
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                DailyRate = v.DailyRate,
                FuelType = v.FuelType,
                Transmission = v.Transmission,
                SeatingCapacity = v.SeatingCapacity,
                IsAvailable = v.IsAvailable
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<VehicleListDto> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = request.RegistrationNumber,
            Vin = request.Vin,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            FuelType = request.FuelType,
            Transmission = request.Transmission,
            SeatingCapacity = request.SeatingCapacity,
            DailyRate = request.DailyRate,
            BranchId = request.BranchId,
            IsAvailable = true
        };

        await _context.Vehicles.AddAsync(vehicle, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleListDto
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            DailyRate = vehicle.DailyRate,
            FuelType = vehicle.FuelType,
            Transmission = vehicle.Transmission,
            SeatingCapacity = vehicle.SeatingCapacity,
            IsAvailable = vehicle.IsAvailable
        };
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FindAsync(id, cancellationToken);
        if (vehicle is null)
        {
            return false;
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
