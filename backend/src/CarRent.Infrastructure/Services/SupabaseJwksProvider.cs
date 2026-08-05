using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace CarRent.Infrastructure.Services;

public class SupabaseJwksProvider
{
    private const string CacheKey = "supabase-jwks";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly string _jwksUrl;

    public SupabaseJwksProvider(IMemoryCache cache, HttpClient httpClient, string jwksUrl)
    {
        _cache = cache;
        _httpClient = httpClient;
        _jwksUrl = jwksUrl;
    }

    public IEnumerable<SecurityKey> GetSigningKeys(string kid)
    {
        var jwks = _cache.GetOrCreate(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var json = _httpClient.GetStringAsync(_jwksUrl).GetAwaiter().GetResult();
            return new JsonWebKeySet(json);
        })!;

        var matches = jwks.Keys.Where(key => string.Equals(key.Kid, kid, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count > 0)
        {
            return matches;
        }

        // Key not found in cache — could be rotation, force a refresh once.
        _cache.Remove(CacheKey);
        var refreshed = _cache.GetOrCreate(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var json = _httpClient.GetStringAsync(_jwksUrl).GetAwaiter().GetResult();
            return new JsonWebKeySet(json);
        })!;

        return refreshed.Keys.Where(key => string.Equals(key.Kid, kid, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
