using CarRent.Application.DTOs.Fleet;

namespace CarRent.Application.Interfaces;

public interface IFleetService
{
    Task<IEnumerable<FleetAvailabilityDto>> GetAvailabilityAsync(CancellationToken cancellationToken = default);
    Task<FleetDashboardSummaryDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
