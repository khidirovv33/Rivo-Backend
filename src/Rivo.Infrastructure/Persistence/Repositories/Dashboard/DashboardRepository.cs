using Microsoft.EntityFrameworkCore;
using Rivo.Application.Dashboard.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.Infrastructure.Persistence.Repositories.Dashboard;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(decimal Total, int Count)> GetSalesSummaryAsync(
        Guid tenantId, Guid? branchId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var query = OrdersInRange(tenantId, branchId, fromUtc, toUtc);

        var result = await query
            .GroupBy(_ => 1)
            .Select(g => new { Total = g.Sum(o => o.TotalAmount), Count = g.Count() })
            .FirstOrDefaultAsync(cancellationToken);

        return (result?.Total ?? 0m, result?.Count ?? 0);
    }

    public async Task<List<(DateOnly Date, decimal Total)>> GetDailySalesAsync(
        Guid tenantId, Guid? branchId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var rows = await OrdersInRange(tenantId, branchId, fromUtc, toUtc)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync(cancellationToken);

        return rows.Select(r => (DateOnly.FromDateTime(r.Date), r.Total)).ToList();
    }

    public async Task<List<(Guid ProductId, string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(
        Guid tenantId, Guid? branchId, DateTime fromUtc, DateTime toUtc, int take, CancellationToken cancellationToken = default)
    {
        var rows = await OrdersInRange(tenantId, branchId, fromUtc, toUtc)
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.Product.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Name,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal),
            })
            .OrderByDescending(x => x.Quantity)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(r => (r.ProductId, r.Name, r.Quantity, r.Revenue)).ToList();
    }

    public async Task<(int ProductCount, int WarehouseCount)> GetLowStockSummaryAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        var lowStockProductIds = await _context.Products
            .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.Status == ProductStatus.Active && p.MinimumStock > 0)
            .Select(p => new
            {
                p.Id,
                p.MinimumStock,
                Available = _context.Stocks
                    .Where(s => s.TenantId == tenantId && s.ProductId == p.Id)
                    .Sum(s => s.SystemQuantity - s.ReservedQuantity),
            })
            .Where(x => x.Available < x.MinimumStock)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (lowStockProductIds.Count == 0)
        {
            return (0, 0);
        }

        var warehouseCount = await _context.Stocks
            .Where(s => s.TenantId == tenantId && lowStockProductIds.Contains(s.ProductId))
            .Select(s => s.WarehouseId)
            .Distinct()
            .CountAsync(cancellationToken);

        return (lowStockProductIds.Count, warehouseCount);
    }

    private IQueryable<Domain.Entities.Orders.Order> OrdersInRange(Guid tenantId, Guid? branchId, DateTime fromUtc, DateTime toUtc)
    {
        var query = _context.Orders.Where(o =>
            o.TenantId == tenantId &&
            o.Status != OrderStatus.Voided &&
            o.CreatedAt >= fromUtc &&
            o.CreatedAt < toUtc);

        if (branchId.HasValue)
        {
            query = query.Where(o => o.BranchId == branchId.Value);
        }

        return query;
    }
}
