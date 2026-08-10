using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class TenantBrandingService : ITenantBrandingService
{
    private readonly CarRentDbContext _context;

    public TenantBrandingService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<TenantBrandingDto> GetAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var branding = await _context.TenantBrandings.AsNoTracking().FirstOrDefaultAsync(b => b.TenantId == tenantId, cancellationToken);
        return branding is null
            ? new TenantBrandingDto(tenantId, null, null, null, null, null, null)
            : MapDto(branding);
    }

    public async Task<TenantBrandingDto> UpsertAsync(Guid tenantId, UpsertTenantBrandingRequest request, CancellationToken cancellationToken = default)
    {
        var branding = await _context.TenantBrandings.FirstOrDefaultAsync(b => b.TenantId == tenantId, cancellationToken);
        if (branding is null)
        {
            branding = new TenantBranding { Id = Guid.NewGuid(), TenantId = tenantId };
            await _context.TenantBrandings.AddAsync(branding, cancellationToken);
        }

        branding.LogoUrl = request.LogoUrl;
        branding.PrimaryColor = request.PrimaryColor;
        branding.SecondaryColor = request.SecondaryColor;
        branding.CompanyDisplayName = request.CompanyDisplayName;
        branding.FaviconUrl = request.FaviconUrl;
        branding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return MapDto(branding);
    }

    private static TenantBrandingDto MapDto(TenantBranding branding) => new(
        branding.TenantId, branding.LogoUrl, branding.PrimaryColor, branding.SecondaryColor, branding.CompanyDisplayName, branding.FaviconUrl, branding.UpdatedAt);
}
