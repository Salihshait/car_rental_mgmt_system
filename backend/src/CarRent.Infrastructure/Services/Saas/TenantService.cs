using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class TenantService : ITenantService
{
    private readonly CarRentDbContext _context;

    public TenantService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<TenantDto> RegisterAsync(RegisterTenantRequest request, CancellationToken cancellationToken = default)
    {
        var slugTaken = await _context.Tenants.AsNoTracking().AnyAsync(t => t.Slug == request.Slug, cancellationToken);
        if (slugTaken)
        {
            throw new InvalidOperationException($"Slug '{request.Slug}' is already in use.");
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName,
            Slug = request.Slug,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Status = "Trial",
            TrialEndsAt = DateTime.UtcNow.AddDays(14)
        };

        await _context.Tenants.AddAsync(tenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapDto(tenant);
    }

    public async Task<IEnumerable<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _context.Tenants.AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);
        return tenants.Select(MapDto);
    }

    public async Task<TenantDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Tenant not found.");
        return MapDto(tenant);
    }

    public async Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Tenant not found.");

        tenant.CompanyName = request.CompanyName;
        tenant.ContactEmail = request.ContactEmail;
        tenant.ContactPhone = request.ContactPhone;
        tenant.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);
        return MapDto(tenant);
    }

    public async Task<TenantDatabaseInfoDto> GetDatabaseInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscriptionCount = await _context.Subscriptions.AsNoTracking().CountAsync(s => s.TenantId == id, cancellationToken);
        var invoiceCount = await _context.SubscriptionInvoices.AsNoTracking().CountAsync(i => i.TenantId == id, cancellationToken);
        var usageMetricCount = await _context.TenantUsageMetrics.AsNoTracking().CountAsync(m => m.TenantId == id, cancellationToken);

        return new TenantDatabaseInfoDto(
            "Shared database, row-level isolation via tenant_id (no dedicated database is provisioned per tenant)",
            subscriptionCount,
            invoiceCount,
            usageMetricCount);
    }

    private static TenantDto MapDto(Tenant tenant) => new(
        tenant.Id, tenant.CompanyName, tenant.Slug, tenant.ContactEmail, tenant.ContactPhone, tenant.Status, tenant.TrialEndsAt, tenant.CreatedAt);
}
