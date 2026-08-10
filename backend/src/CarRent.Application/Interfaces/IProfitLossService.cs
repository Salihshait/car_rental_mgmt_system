using CarRent.Application.DTOs.Finance;

namespace CarRent.Application.Interfaces;

public interface IProfitLossService
{
    Task<ProfitLossDto> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
