using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class TenantDomainService : ITenantDomainService
{
    private readonly CarRentDbContext _context;

    public TenantDomainService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TenantDomainDto>> GetForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var domains = await _context.TenantDomains.AsNoTracking()
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return domains.Select(MapDto);
    }

    public async Task<TenantDomainDto> CreateAsync(Guid tenantId, CreateTenantDomainRequest request, CancellationToken cancellationToken = default)
    {
        var domain = new TenantDomain
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Domain = request.Domain,
            Status = "Pending"
        };

        await _context.TenantDomains.AddAsync(domain, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapDto(domain);
    }

    public async Task<TenantDomainDto> VerifyAsync(Guid domainId, CancellationToken cancellationToken = default)
    {
        var domain = await _context.TenantDomains.FirstOrDefaultAsync(d => d.Id == domainId, cancellationToken)
            ?? throw new InvalidOperationException("Domain not found.");

        domain.Status = "Verified";
        await _context.SaveChangesAsync(cancellationToken);

        return MapDto(domain);
    }

    private static TenantDomainDto MapDto(TenantDomain domain) => new(domain.Id, domain.TenantId, domain.Domain, domain.Status, domain.CreatedAt);
}
