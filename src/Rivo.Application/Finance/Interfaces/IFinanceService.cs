using Rivo.Application.Finance.Dtos;

namespace Rivo.Application.Finance.Interfaces;

public interface IFinanceService
{
    Task<FinanceSummaryDto> GetSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
