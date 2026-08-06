using CarRent.Application.DTOs.Vehicles;

namespace CarRent.Application.Interfaces;

public interface IAvailabilityService
{
    Task<bool> IsVehicleAvailableAsync(Guid vehicleId, DateTime startDate, DateTime endDate, Guid? excludeBookingId = null, CancellationToken cancellationToken = default);
    Task<VehicleAvailabilityDto> GetVehicleAvailabilityAsync(Guid vehicleId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<VehicleAvailabilityDto>> GetAvailabilityCalendarAsync(Guid? branchId, Guid? categoryId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
