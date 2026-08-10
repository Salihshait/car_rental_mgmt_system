using CarRent.Application.DTOs.Finance;

namespace CarRent.Application.Interfaces;

public interface IBalanceSheetService
{
    Task<BalanceSheetDto> GetAsync(DateTime? asOfDate, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAsync(string format, DateTime? asOfDate, CancellationToken cancellationToken = default);
}
