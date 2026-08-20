using Microsoft.EntityFrameworkCore;
using Rivo.Application.Analytics.Dtos;
using Rivo.Application.Analytics.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.Application.Analytics.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;

    public AnalyticsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalesTrendPointDto>> GetSalesTrendAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var salesByDay = await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.Status != OrderStatus.Voided)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Sales = g.Sum(o => o.TotalAmount), Orders = g.Count() })
            .ToListAsync(cancellationToken);

        var cogsByDay = await _context.OrderItems.AsNoTracking()
            .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to && oi.Order.Status != OrderStatus.Voided)
            .GroupBy(oi => oi.Order.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Cogs = g.Sum(oi => oi.Quantity * oi.Product.PurchasePrice) })
            .ToDictionaryAsync(x => x.Date, x => x.Cogs, cancellationToken);

        return salesByDay
            .Select(x => new SalesTrendPointDto
            {
                Date = DateOnly.FromDateTime(x.Date),
                Sales = x.Sales,
                Profit = x.Sales - cogsByDay.GetValueOrDefault(x.Date),
                OrdersCount = x.Orders,
            })
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<List<ProductRankingDto>> GetBestSellersAsync(DateTime from, DateTime to, int top, CancellationToken cancellationToken = default)
    {
        var ranking = await RankProductsAsync(from, to, cancellationToken);
        return ranking.OrderByDescending(x => x.QuantitySold).Take(top).ToList();
    }

    public async Task<List<ProductRankingDto>> GetMostProfitableAsync(DateTime from, DateTime to, int top, CancellationToken cancellationToken = default)
    {
        var ranking = await RankProductsAsync(from, to, cancellationToken);
        return ranking.OrderByDescending(x => x.Profit).Take(top).ToList();
    }

    public async Task<List<SlowMovingProductDto>> GetSlowMovingAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var sold = await _context.OrderItems.AsNoTracking()
            .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to && oi.Order.Status != OrderStatus.Voided)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => (decimal)x.Quantity, cancellationToken);

        var stockByProduct = await CurrentStockByProductAsync(cancellationToken);

        // "Slow moving" = has stock, but sold little/nothing during the window.
        var slowMoving = stockByProduct
            .Where(s => s.Value > 0)
            .Select(s => new SlowMovingProductDto
            {
                ProductId = s.Key,
                CurrentStock = s.Value,
                QuantitySoldInPeriod = sold.GetValueOrDefault(s.Key),
            })
            .OrderBy(x => x.QuantitySoldInPeriod)
            .Take(50)
            .ToList();

        await AttachProductNames(slowMoving, x => x.ProductId, (x, name) => x.ProductName = name, cancellationToken);
        return slowMoving;
    }

    public async Task<List<SlowMovingProductDto>> GetDeadStockAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        var soldProductIds = await _context.OrderItems.AsNoTracking()
            .Where(oi => oi.Order.CreatedAt >= since && oi.Order.Status != OrderStatus.Voided)
            .Select(oi => oi.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var stockByProduct = await CurrentStockByProductAsync(cancellationToken);
        var soldSet = soldProductIds.ToHashSet();

        var deadStock = stockByProduct
            .Where(s => s.Value > 0 && !soldSet.Contains(s.Key))
            .Select(s => new SlowMovingProductDto { ProductId = s.Key, CurrentStock = s.Value, QuantitySoldInPeriod = 0 })
            .ToList();

        await AttachProductNames(deadStock, x => x.ProductId, (x, name) => x.ProductName = name, cancellationToken);
        return deadStock;
    }

    public async Task<List<LowStockItemDto>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products.AsNoTracking()
            .Where(p => p.MinimumStock > 0)
            .Select(p => new { p.Id, p.Name, p.MinimumStock })
            .ToListAsync(cancellationToken);

        var stockByProduct = await CurrentStockByProductAsync(cancellationToken);

        return products
            .Select(p => new LowStockItemDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CurrentStock = stockByProduct.GetValueOrDefault(p.Id),
                MinimumStock = p.MinimumStock,
            })
            .Where(x => x.CurrentStock <= x.MinimumStock)
            .OrderBy(x => x.CurrentStock)
            .ToList();
    }

    public async Task<List<EmployeeStatDto>> GetEmployeeStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var stats = await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.Status != OrderStatus.Voided)
            .GroupBy(o => new { o.CashierUserId, o.CashierUser.FullName })
            .Select(g => new EmployeeStatDto
            {
                UserId = g.Key.CashierUserId,
                UserName = g.Key.FullName,
                OrdersCount = g.Count(),
                TotalSales = g.Sum(o => o.TotalAmount),
                AverageCheck = g.Average(o => o.TotalAmount),
            })
            .OrderByDescending(x => x.TotalSales)
            .ToListAsync(cancellationToken);

        return stats;
    }

    public async Task<List<BranchComparisonDto>> GetBranchComparisonAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var sales = await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.Status != OrderStatus.Voided)
            .GroupBy(o => new { o.BranchId, o.Branch.Name })
            .Select(g => new { g.Key.BranchId, g.Key.Name, Orders = g.Count(), Sales = g.Sum(o => o.TotalAmount) })
            .ToListAsync(cancellationToken);

        var cogsByBranch = await _context.OrderItems.AsNoTracking()
            .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to && oi.Order.Status != OrderStatus.Voided)
            .GroupBy(oi => oi.Order.BranchId)
            .Select(g => new { BranchId = g.Key, Cogs = g.Sum(oi => oi.Quantity * oi.Product.PurchasePrice) })
            .ToDictionaryAsync(x => x.BranchId, x => x.Cogs, cancellationToken);

        return sales
            .Select(x => new BranchComparisonDto
            {
                BranchId = x.BranchId,
                BranchName = x.Name,
                OrdersCount = x.Orders,
                TotalSales = x.Sales,
                TotalProfit = x.Sales - cogsByBranch.GetValueOrDefault(x.BranchId),
            })
            .OrderByDescending(x => x.TotalSales)
            .ToList();
    }

    private async Task<List<ProductRankingDto>> RankProductsAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return await _context.OrderItems.AsNoTracking()
            .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to && oi.Order.Status != OrderStatus.Voided)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new ProductRankingDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.LineTotal),
                Profit = g.Sum(oi => oi.LineTotal - (oi.Quantity * oi.Product.PurchasePrice)),
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, decimal>> CurrentStockByProductAsync(CancellationToken cancellationToken)
    {
        return await _context.Stocks.AsNoTracking()
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(s => s.SystemQuantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Quantity, cancellationToken);
    }

    private async Task AttachProductNames<T>(
        List<T> items, Func<T, Guid> productIdSelector, Action<T, string> nameSetter, CancellationToken cancellationToken)
    {
        var ids = items.Select(productIdSelector).Distinct().ToList();
        var names = await _context.Products.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        foreach (var item in items)
        {
            nameSetter(item, names.GetValueOrDefault(productIdSelector(item), "?"));
        }
    }
}
