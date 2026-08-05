using System.Security.Claims;
using CarRent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class UserClaimsTransformation : IClaimsTransformation
{
    private readonly CarRentDbContext _context;

    public UserClaimsTransformation(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true || principal.HasClaim(c => c.Type == ClaimTypes.Role))
        {
            return principal;
        }

        var subject = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (!Guid.TryParse(subject, out var userId))
        {
            return principal;
        }

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null || user.Role is null)
        {
            return principal;
        }

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.Name));
        identity.AddClaim(new Claim("app_user_id", user.Id.ToString()));
        principal.AddIdentity(identity);

        return principal;
    }
}
