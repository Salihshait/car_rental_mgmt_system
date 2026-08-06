using CarRent.Application.DTOs.Drivers;

namespace CarRent.Application.Interfaces;

public interface IDriverAttendanceService
{
    Task<DriverAttendanceDto> CheckInAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<DriverAttendanceDto> CheckOutAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DriverAttendanceDto>> GetAttendanceAsync(Guid driverId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<DriverAttendanceDto> MarkAsync(Guid driverId, MarkAttendanceRequest request, CancellationToken cancellationToken = default);
}
