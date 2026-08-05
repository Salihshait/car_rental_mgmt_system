namespace CarRent.Application.Interfaces;

public record SupabaseAdminUser(Guid Id, string Email);

public interface ISupabaseAdminClient
{
    Task<SupabaseAdminUser> InviteUserAsync(string email, CancellationToken cancellationToken = default);
    Task LogoutAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string> UploadFileAsync(string bucket, string path, byte[] content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string bucket, string path, CancellationToken cancellationToken = default);
}
