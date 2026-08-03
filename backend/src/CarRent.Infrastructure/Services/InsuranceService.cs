using CarRent.Application.DTOs.Insurance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class InsuranceService : IInsuranceService
{
    private readonly CarRentDbContext _context;

    public InsuranceService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InsuranceSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Insurances
            .AsNoTracking()
            .Select(i => new InsuranceSummaryDto
            {
                Id = i.Id,
                VehicleId = i.VehicleId,
                PolicyNumber = i.PolicyNumber,
                ExpiryDate = i.ExpiryDate,
                Provider = i.Provider
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InsuranceSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Insurances
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InsuranceSummaryDto
            {
                Id = i.Id,
                VehicleId = i.VehicleId,
                PolicyNumber = i.PolicyNumber,
                ExpiryDate = i.ExpiryDate,
                Provider = i.Provider
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InsuranceSummaryDto> CreateAsync(CreateInsuranceRequest request, CancellationToken cancellationToken = default)
    {
        var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == request.VehicleId, cancellationToken);
        if (!vehicleExists)
        {
            throw new InvalidOperationException("The selected vehicle does not exist.");
        }

        var insurance = new Insurance
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            PolicyNumber = request.PolicyNumber,
            ExpiryDate = request.ExpiryDate,
            Provider = request.Provider
        };

        await _context.Insurances.AddAsync(insurance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new InsuranceSummaryDto
        {
            Id = insurance.Id,
            VehicleId = insurance.VehicleId,
            PolicyNumber = insurance.PolicyNumber,
            ExpiryDate = insurance.ExpiryDate,
            Provider = insurance.Provider
        };
    }
}
