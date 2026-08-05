using CarRent.Application.DTOs.Users;

namespace CarRent.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserSummaryDto> CompleteProfileAsync(Guid userId, string email, CompleteProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserSummaryDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserSummaryDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserSummaryDto> UpdateStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task<UserSummaryDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserSummaryDto> SetAvatarAsync(Guid id, string avatarUrl, CancellationToken cancellationToken = default);
}
