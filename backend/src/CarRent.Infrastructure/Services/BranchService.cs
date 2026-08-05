using CarRent.Application.DTOs.Branches;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly CarRentDbContext _context;

    public BranchService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BranchSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .AsNoTracking()
            .Select(b => new BranchSummaryDto { Id = b.Id, Name = b.Name, City = b.City, IsActive = b.IsActive })
            .ToListAsync(cancellationToken);
    }
}
