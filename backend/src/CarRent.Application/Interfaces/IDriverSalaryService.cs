using CarRent.Application.DTOs.Drivers;

namespace CarRent.Application.Interfaces;

public interface IDriverSalaryService
{
    Task<IEnumerable<DriverSalaryPaymentDto>> GetAllAsync(Guid? driverId, CancellationToken cancellationToken = default);
    Task<DriverSalaryPaymentDto> GenerateAsync(CreateSalaryPaymentRequest request, Guid createdBy, CancellationToken cancellationToken = default);
    Task<DriverSalaryPaymentDto> MarkPaidAsync(Guid id, CancellationToken cancellationToken = default);
}
