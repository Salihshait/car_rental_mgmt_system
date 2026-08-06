using CarRent.Application.DTOs.Drivers;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class DriverRatingService : IDriverRatingService
{
    private readonly CarRentDbContext _context;

    public DriverRatingService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DriverRatingDto>> GetAllAsync(Guid? driverId, CancellationToken cancellationToken = default)
    {
        var query = _context.DriverRatings.AsNoTracking().AsQueryable();

        if (driverId.HasValue)
        {
            query = query.Where(r => r.DriverId == driverId);
        }

        var ratings = await query.OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);

        var raterIds = ratings.Select(r => r.RatedBy).Distinct().ToList();
        var raters = await _context.Users.AsNoTracking().Where(u => raterIds.Contains(u.Id)).ToListAsync(cancellationToken);

        return ratings.Select(r =>
        {
            var rater = raters.FirstOrDefault(u => u.Id == r.RatedBy);
            return new DriverRatingDto
            {
                Id = r.Id,
                DriverId = r.DriverId,
                RatedBy = r.RatedBy,
                RatedByName = rater is null ? null : $"{rater.FirstName} {rater.LastName}",
                Score = r.Score,
                Category = r.Category,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            };
        }).ToList();
    }

    public async Task<DriverRatingDto> AddAsync(CreateDriverRatingRequest request, Guid ratedBy, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken)
            ?? throw new InvalidOperationException("Driver not found.");

        if (request.Score is < 1 or > 5)
        {
            throw new InvalidOperationException("Score must be between 1 and 5.");
        }

        var rating = new DriverRating
        {
            Id = Guid.NewGuid(),
            DriverId = request.DriverId,
            RatedBy = ratedBy,
            Score = request.Score,
            Category = request.Category,
            Comment = request.Comment
        };

        await _context.DriverRatings.AddAsync(rating, cancellationToken);

        var allScores = await _context.DriverRatings
            .Where(r => r.DriverId == request.DriverId)
            .Select(r => r.Score)
            .ToListAsync(cancellationToken);
        allScores.Add(request.Score);

        driver.Rating = Math.Round((decimal)allScores.Average(), 2);

        await _context.SaveChangesAsync(cancellationToken);

        var rater = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == ratedBy, cancellationToken);

        return new DriverRatingDto
        {
            Id = rating.Id,
            DriverId = rating.DriverId,
            RatedBy = rating.RatedBy,
            RatedByName = rater is null ? null : $"{rater.FirstName} {rater.LastName}",
            Score = rating.Score,
            Category = rating.Category,
            Comment = rating.Comment,
            CreatedAt = rating.CreatedAt
        };
    }
}
