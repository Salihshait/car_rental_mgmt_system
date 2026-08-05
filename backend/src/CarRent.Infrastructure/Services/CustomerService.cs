using CarRent.Application.DTOs.Customers;
using CarRent.Application.DTOs.Vehicles;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly CarRentDbContext _context;
    private readonly ISupabaseAdminClient _supabaseAdminClient;

    public CustomerService(CarRentDbContext context, ISupabaseAdminClient supabaseAdminClient)
    {
        _context = context;
        _supabaseAdminClient = supabaseAdminClient;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CustomerFilter filter, CancellationToken cancellationToken = default)
    {
        var query = Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(c =>
                c.FirstName.Contains(term) || c.LastName.Contains(term) || c.Email.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.KycStatus))
        {
            query = query.Where(c => c.KycStatus == filter.KycStatus);
        }

        if (filter.IsBlacklisted.HasValue)
        {
            query = query.Where(c => c.IsBlacklisted == filter.IsBlacklisted);
        }

        if (filter.IsCorporate.HasValue)
        {
            query = query.Where(c => c.IsCorporate == filter.IsCorporate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(c => c.Id == userId, cancellationToken);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer", cancellationToken)
            ?? throw new InvalidOperationException("Default Customer role is not seeded.");

        var invited = await _supabaseAdminClient.InviteUserAsync(request.Email, cancellationToken);

        var user = new User
        {
            Id = invited.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = invited.Email,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            IsEmailVerified = false,
            RoleId = customerRole.Id
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await EnsureCustomerRecordAsync(user.Id, cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created customer.");
    }

    public async Task EnsureCustomerRecordAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Customers.AnyAsync(c => c.UserId == userId, cancellationToken);
        if (exists)
        {
            return;
        }

        await _context.Customers.AddAsync(new Customer { Id = Guid.NewGuid(), UserId = userId }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerDto> UpdateAsync(Guid userId, UpdateCustomerRequest request, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        if (customer.KycStatus != request.KycStatus)
        {
            await WriteAuditAsync(userId, "KycStatusChanged", $"KYC status changed from {customer.KycStatus} to {request.KycStatus}.", actingUserId, cancellationToken);
            await WriteNotificationAsync(userId, "Kyc", $"Your KYC status is now {request.KycStatus}.", cancellationToken);
        }

        if (customer.IsBlacklisted != request.IsBlacklisted)
        {
            await WriteAuditAsync(userId, "BlacklistChanged", request.IsBlacklisted ? "Customer blacklisted." : "Customer removed from blacklist.", actingUserId, cancellationToken);
            await WriteNotificationAsync(userId, "Blacklist", request.IsBlacklisted ? "Your account has been blacklisted." : "Your account is no longer blacklisted.", cancellationToken);
        }

        customer.KycStatus = request.KycStatus;
        customer.IsBlacklisted = request.IsBlacklisted;
        customer.IsCorporate = request.IsCorporate;
        customer.CompanyName = request.CompanyName;
        customer.EmergencyContactName = request.EmergencyContactName;
        customer.EmergencyContactPhone = request.EmergencyContactPhone;
        customer.EmergencyContactRelation = request.EmergencyContactRelation;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated customer.");
    }

    public async Task<CustomerDto> UpdateEmergencyContactAsync(Guid userId, UpdateEmergencyContactRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer record not found for this account.");

        customer.EmergencyContactName = request.EmergencyContactName;
        customer.EmergencyContactPhone = request.EmergencyContactPhone;
        customer.EmergencyContactRelation = request.EmergencyContactRelation;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated customer.");
    }

    public async Task<CustomerDto> AdjustWalletAsync(Guid userId, WalletAdjustRequest request, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        customer.WalletBalance += request.Amount;
        if (customer.WalletBalance < 0)
        {
            throw new InvalidOperationException("Wallet balance cannot go negative.");
        }

        var direction = request.Amount >= 0 ? "credited" : "debited";
        await WriteAuditAsync(userId, "WalletAdjusted", $"Wallet {direction} {Math.Abs(request.Amount)}. Reason: {request.Reason}. New balance: {customer.WalletBalance}.", actingUserId, cancellationToken);
        await WriteNotificationAsync(userId, "Wallet", $"Your wallet was {direction} {Math.Abs(request.Amount)} ({request.Reason}).", cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated customer.");
    }

    public async Task<CustomerDto> AdjustLoyaltyAsync(Guid userId, LoyaltyAdjustRequest request, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        customer.LoyaltyPoints += request.Points;
        if (customer.LoyaltyPoints < 0)
        {
            throw new InvalidOperationException("Loyalty points cannot go negative.");
        }

        var direction = request.Points >= 0 ? "earned" : "redeemed";
        await WriteAuditAsync(userId, "LoyaltyAdjusted", $"{Math.Abs(request.Points)} points {direction}. Reason: {request.Reason}. New total: {customer.LoyaltyPoints}.", actingUserId, cancellationToken);
        await WriteNotificationAsync(userId, "Loyalty", $"You {direction} {Math.Abs(request.Points)} loyalty points ({request.Reason}).", cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated customer.");
    }

    public async Task<IEnumerable<VehicleTimelineEntryDto>> GetTimelineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == "Customer" && a.EntityId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new VehicleTimelineEntryDto { Id = a.Id, Action = a.Action, Payload = a.Payload, CreatedAt = a.CreatedAt })
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _context.Customers.AsNoTracking().ToListAsync(cancellationToken);

        return new CustomerDashboardDto
        {
            TotalCustomers = customers.Count,
            BlacklistedCount = customers.Count(c => c.IsBlacklisted),
            CorporateCount = customers.Count(c => c.IsCorporate),
            TotalWalletBalance = customers.Sum(c => c.WalletBalance),
            TotalLoyaltyPoints = customers.Sum(c => c.LoyaltyPoints),
            KycStatusCounts = customers.GroupBy(c => c.KycStatus).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IEnumerable<FavoriteVehicleDto>> GetFavoritesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.FavoriteVehicles
            .AsNoTracking()
            .Where(f => f.CustomerId == userId)
            .Join(_context.Vehicles, f => f.VehicleId, v => v.Id, (f, v) => v)
            .Include(v => v.Brand)
            .Include(v => v.VehicleModel)
            .Select(v => new FavoriteVehicleDto
            {
                VehicleId = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                BrandName = v.Brand != null ? v.Brand.Name : null,
                ModelName = v.VehicleModel != null ? v.VehicleModel.Name : null,
                DailyRate = v.DailyRate,
                Status = v.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AddFavoriteAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == vehicleId, cancellationToken);
        if (!vehicleExists)
        {
            throw new InvalidOperationException("Vehicle not found.");
        }

        var alreadyFavorited = await _context.FavoriteVehicles.AnyAsync(f => f.CustomerId == userId && f.VehicleId == vehicleId, cancellationToken);
        if (alreadyFavorited)
        {
            return;
        }

        await _context.FavoriteVehicles.AddAsync(new FavoriteVehicle { CustomerId = userId, VehicleId = vehicleId }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFavoriteAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var favorite = await _context.FavoriteVehicles.FirstOrDefaultAsync(f => f.CustomerId == userId && f.VehicleId == vehicleId, cancellationToken);
        if (favorite is null)
        {
            return;
        }

        _context.FavoriteVehicles.Remove(favorite);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task WriteAuditAsync(Guid userId, string action, string message, Guid? actingUserId, CancellationToken cancellationToken)
    {
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = actingUserId,
            Action = action,
            EntityType = "Customer",
            EntityId = userId,
            Payload = $"{{\"message\":\"{message.Replace("\"", "'")}\"}}"
        }, cancellationToken);
    }

    private async Task WriteNotificationAsync(Guid userId, string type, string message, CancellationToken cancellationToken)
    {
        await _context.Notifications.AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NotificationType = type,
            Message = message
        }, cancellationToken);
    }

    private IQueryable<CustomerDto> Query()
    {
        return _context.Users
            .AsNoTracking()
            .Where(u => u.Role.Name == "Customer")
            .Join(_context.Customers, u => u.Id, c => c.UserId, (u, c) => new CustomerDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                KycStatus = c.KycStatus,
                WalletBalance = c.WalletBalance,
                LoyaltyPoints = c.LoyaltyPoints,
                IsBlacklisted = c.IsBlacklisted,
                IsCorporate = c.IsCorporate,
                CompanyName = c.CompanyName,
                EmergencyContactName = c.EmergencyContactName,
                EmergencyContactPhone = c.EmergencyContactPhone,
                EmergencyContactRelation = c.EmergencyContactRelation,
                CreatedAt = c.CreatedAt
            });
    }
}
