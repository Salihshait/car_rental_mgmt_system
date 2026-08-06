using CarRent.Application.DTOs.Drivers;

namespace CarRent.Application.Interfaces;

public interface IDriverRatingService
{
    Task<IEnumerable<DriverRatingDto>> GetAllAsync(Guid? driverId, CancellationToken cancellationToken = default);
    Task<DriverRatingDto> AddAsync(CreateDriverRatingRequest request, Guid ratedBy, CancellationToken cancellationToken = default);
}
