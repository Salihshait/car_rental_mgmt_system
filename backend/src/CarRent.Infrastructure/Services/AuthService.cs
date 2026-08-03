using CarRent.Application.DTOs.Auth;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly CarRentDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(CarRentDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists)
        {
            return new AuthResultDto { IsSuccess = false, Message = "Email already exists." };
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer", cancellationToken)
            ?? new Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Default customer role" };

        if (role.Id == Guid.Empty)
        {
            role.Id = Guid.NewGuid();
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsEmailVerified = true,
            Role = role,
            RoleId = role.Id
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            IsSuccess = true,
            AccessToken = _jwtTokenService.GenerateAccessToken(user, new[] { role.Name }),
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(60),
            Message = "Registration successful."
        };
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || user.Role is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new AuthResultDto { IsSuccess = false, Message = "Invalid credentials." };
        }

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            IsSuccess = true,
            AccessToken = _jwtTokenService.GenerateAccessToken(user, new[] { user.Role.Name }),
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(60),
            Message = "Login successful."
        };
    }

    public async Task<AuthResultDto> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResultDto { IsSuccess = false, Message = "Refresh token expired or invalid." };
        }

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            IsSuccess = true,
            AccessToken = _jwtTokenService.GenerateAccessToken(user, new[] { user.Role.Name }),
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(60),
            Message = "Token refreshed successfully."
        };
    }
}
