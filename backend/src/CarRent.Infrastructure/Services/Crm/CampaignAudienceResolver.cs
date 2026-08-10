using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Crm;

internal static class CampaignAudienceResolver
{
    public static async Task<List<(User User, Customer Customer)>> ResolveAsync(CarRentDbContext context, string audienceFilter, CancellationToken cancellationToken)
    {
        var customersQuery = context.Customers.AsNoTracking().Where(c => !c.IsBlacklisted).AsQueryable();
        customersQuery = audienceFilter switch
        {
            "CorporateOnly" => customersQuery.Where(c => c.IsCorporate),
            "IndividualOnly" => customersQuery.Where(c => !c.IsCorporate),
            _ => customersQuery
        };

        var customers = await customersQuery.ToListAsync(cancellationToken);
        var userIds = customers.Select(c => c.UserId).ToList();
        var users = await context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);

        return customers
            .Select(c => (User: users.FirstOrDefault(u => u.Id == c.UserId), Customer: c))
            .Where(x => x.User is not null)
            .Select(x => (x.User!, x.Customer))
            .ToList();
    }
}
