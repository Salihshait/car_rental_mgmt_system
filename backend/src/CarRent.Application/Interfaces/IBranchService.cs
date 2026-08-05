using CarRent.Application.DTOs.Branches;

namespace CarRent.Application.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<BranchSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
