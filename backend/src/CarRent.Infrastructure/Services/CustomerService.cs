using CarRent.Application.DTOs.Customers;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly CarRentDbContext _context;

    public CustomerService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role != null && u.Role.Name == "Customer")
            .Select(u => new CustomerSummaryDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                IsActive = u.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new CustomerSummaryDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CustomerSummaryDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer", cancellationToken)
            ?? new Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Default customer role" };

        var customer = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            IsEmailVerified = true,
            RoleId = customerRole.Id,
            Role = customerRole
        };

        await _context.Users.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerSummaryDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber ?? string.Empty,
            IsActive = customer.IsActive
        };
    }
}
