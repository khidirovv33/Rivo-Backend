using Rivo.Application.Analytics.Dtos;

namespace Rivo.Application.Analytics.Interfaces;

public interface IAnalyticsService
{
    Task<List<SalesTrendPointDto>> GetSalesTrendAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<List<ProductRankingDto>> GetBestSellersAsync(DateTime from, DateTime to, int top, CancellationToken cancellationToken = default);

    Task<List<ProductRankingDto>> GetMostProfitableAsync(DateTime from, DateTime to, int top, CancellationToken cancellationToken = default);

    Task<List<SlowMovingProductDto>> GetSlowMovingAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<List<SlowMovingProductDto>> GetDeadStockAsync(DateTime since, CancellationToken cancellationToken = default);

    Task<List<LowStockItemDto>> GetLowStockAsync(CancellationToken cancellationToken = default);

    Task<List<EmployeeStatDto>> GetEmployeeStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<List<BranchComparisonDto>> GetBranchComparisonAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
