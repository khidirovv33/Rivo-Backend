namespace Rivo.Application.Dashboard.Interfaces;

public interface IDashboardRepository
{
    Task<(decimal Total, int Count)> GetSalesSummaryAsync(
        Guid tenantId, Guid? branchId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    Task<List<(DateOnly Date, decimal Total)>> GetDailySalesAsync(
        Guid tenantId, Guid? branchId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    Task<List<(Guid ProductId, string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(
        Guid tenantId, Guid? branchId, DateTime fromUtc, DateTime toUtc, int take, CancellationToken cancellationToken = default);

    /// <summary>Товары, у которых доступный остаток по всем складам тенанта ниже Product.MinimumStock.</summary>
    Task<(int ProductCount, int WarehouseCount)> GetLowStockSummaryAsync(
        Guid tenantId, CancellationToken cancellationToken = default);
}
