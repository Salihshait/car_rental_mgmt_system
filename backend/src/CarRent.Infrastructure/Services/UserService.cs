using CarRent.Application.DTOs.Users;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly CarRentDbContext _context;
    private readonly ISupabaseAdminClient _supabaseAdminClient;
    private readonly ICustomerService _customerService;

    public UserService(CarRentDbContext context, ISupabaseAdminClient supabaseAdminClient, ICustomerService customerService)
    {
        _context = context;
        _supabaseAdminClient = supabaseAdminClient;
        _customerService = customerService;
    }

    public async Task<IEnumerable<UserSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query().ToListAsync(cancellationToken);
    }

    public async Task<UserSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<UserSummaryDto> CompleteProfileAsync(Guid userId, string email, CompleteProfileRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (existing)
        {
            throw new InvalidOperationException("Profile already completed for this account.");
        }

        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer", cancellationToken)
            ?? throw new InvalidOperationException("Default Customer role is not seeded.");

        var user = new User
        {
            Id = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = email,
            PhoneNumber = request.PhoneNumber,
            IsEmailVerified = true,
            IsActive = true,
            RoleId = customerRole.Id
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await _customerService.EnsureCustomerRecordAsync(userId, cancellationToken);

        return await GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("Failed to load created profile.");
    }

    public async Task<UserSummaryDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("Profile not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated profile.");
    }

    public async Task<UserSummaryDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken);
        if (!roleExists)
        {
            throw new InvalidOperationException("The selected role does not exist.");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.RoleId = request.RoleId;
        user.DepartmentId = request.DepartmentId;
        user.BranchId = request.BranchId;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated user.");
    }

    public async Task<UserSummaryDto> UpdateStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated user.");
    }

    public async Task<UserSummaryDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role is null)
        {
            throw new InvalidOperationException("The selected role does not exist.");
        }

        var emailInUse = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailInUse)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var invited = await _supabaseAdminClient.InviteUserAsync(request.Email, cancellationToken);

        var user = new User
        {
            Id = invited.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = invited.Email,
            PhoneNumber = request.PhoneNumber,
            IsEmailVerified = false,
            IsActive = true,
            RoleId = request.RoleId,
            DepartmentId = request.DepartmentId,
            BranchId = request.BranchId
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        if (role.Name == "Customer")
        {
            await _customerService.EnsureCustomerRecordAsync(user.Id, cancellationToken);
        }

        return await GetByIdAsync(user.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created user.");
    }

    public async Task<UserSummaryDto> SetAvatarAsync(Guid id, string avatarUrl, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated user.");
    }

    private IQueryable<UserSummaryDto> Query()
    {
        return _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Include(u => u.Branch)
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                AvatarUrl = u.AvatarUrl,
                IsEmailVerified = u.IsEmailVerified,
                IsActive = u.IsActive,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department != null ? u.Department.Name : null,
                BranchId = u.BranchId,
                BranchName = u.Branch != null ? u.Branch.Name : null,
                CreatedAt = u.CreatedAt
            });
    }
}
