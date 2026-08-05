using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CarRent.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CarRent.Infrastructure.Services;

public class SupabaseAdminClient : ISupabaseAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly string _serviceRoleKey;
    private readonly string _baseUrl;

    public SupabaseAdminClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = (configuration["Supabase:Url"]
            ?? Environment.GetEnvironmentVariable("SUPABASE_URL")
            ?? "https://zjudixjgsyeglevlzrgp.supabase.co").TrimEnd('/');
        _serviceRoleKey = configuration["Supabase:ServiceRoleKey"]
            ?? Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
            ?? string.Empty;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, $"{_baseUrl}{path}");
        request.Headers.Add("apikey", _serviceRoleKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);
        return request;
    }

    public async Task<SupabaseAdminUser> InviteUserAsync(string email, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Post, "/auth/v1/invite");
        request.Content = JsonContent.Create(new { email });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Supabase invite failed: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var id = Guid.Parse(doc.RootElement.GetProperty("id").GetString()!);
        var returnedEmail = doc.RootElement.GetProperty("email").GetString()!;

        return new SupabaseAdminUser(id, returnedEmail);
    }

    public async Task LogoutAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Post, $"/auth/v1/admin/users/{userId}/logout?scope=global");
        request.Content = JsonContent.Create(new { scope = "global" });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Supabase global logout failed: {body}");
        }
    }

    public async Task<string> UploadFileAsync(string bucket, string path, byte[] content, string contentType, CancellationToken cancellationToken = default)
    {
        var objectPath = $"{bucket}/{path}";
        var request = CreateRequest(HttpMethod.Post, $"/storage/v1/object/{objectPath}");
        request.Headers.Add("x-upsert", "true");
        request.Content = new ByteArrayContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Supabase file upload failed: {body}");
        }

        return $"{_baseUrl}/storage/v1/object/public/{objectPath}";
    }

    public async Task DeleteFileAsync(string bucket, string path, CancellationToken cancellationToken = default)
    {
        var objectPath = $"{bucket}/{path}";
        var request = CreateRequest(HttpMethod.Delete, $"/storage/v1/object/{objectPath}");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Supabase file delete failed: {body}");
        }
    }
}
