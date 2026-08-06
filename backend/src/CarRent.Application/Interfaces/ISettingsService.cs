namespace CarRent.Application.Interfaces;

public interface ISettingsService
{
    Task<string?> GetAsync(string keyName, CancellationToken cancellationToken = default);
    Task SetAsync(string keyName, string value, string category, CancellationToken cancellationToken = default);
}
