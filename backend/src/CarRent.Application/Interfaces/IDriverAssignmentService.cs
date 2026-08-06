using CarRent.Application.DTOs.Fleet;

namespace CarRent.Application.Interfaces;

public interface IDriverAssignmentService
{
    Task<IEnumerable<DriverAssignmentDto>> GetHistoryAsync(Guid? vehicleId, Guid? driverId, CancellationToken cancellationToken = default);
    Task<DriverAssignmentDto> AssignAsync(AssignDriverRequest request, Guid assignedBy, CancellationToken cancellationToken = default);
    Task UnassignAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}
