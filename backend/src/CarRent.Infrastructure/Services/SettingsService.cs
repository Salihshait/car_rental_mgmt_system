using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly CarRentDbContext _context;

    public SettingsService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetAsync(string keyName, CancellationToken cancellationToken = default)
    {
        var setting = await _context.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.KeyName == keyName, cancellationToken);
        return setting?.KeyValue;
    }

    public async Task SetAsync(string keyName, string value, string category, CancellationToken cancellationToken = default)
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.KeyName == keyName, cancellationToken);
        if (setting is null)
        {
            await _context.Settings.AddAsync(new Setting { Id = Guid.NewGuid(), KeyName = keyName, KeyValue = value, Category = category }, cancellationToken);
        }
        else
        {
            setting.KeyValue = value;
            setting.Category = category;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
