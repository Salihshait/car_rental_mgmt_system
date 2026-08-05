using CarRent.Application.DTOs.Customers;
using CarRent.Application.DTOs.Vehicles;

namespace CarRent.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync(CustomerFilter filter, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(Guid userId, UpdateCustomerRequest request, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateEmergencyContactAsync(Guid userId, UpdateEmergencyContactRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto> AdjustWalletAsync(Guid userId, WalletAdjustRequest request, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task<CustomerDto> AdjustLoyaltyAsync(Guid userId, LoyaltyAdjustRequest request, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VehicleTimelineEntryDto>> GetTimelineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CustomerDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FavoriteVehicleDto>> GetFavoritesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddFavoriteAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default);
    Task RemoveFavoriteAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default);
    Task EnsureCustomerRecordAsync(Guid userId, CancellationToken cancellationToken = default);
}
