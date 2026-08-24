using Microsoft.EntityFrameworkCore;
using Rivo.Application.Analytics.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Dashboard.Dtos;
using Rivo.Application.Dashboard.Interfaces;
using Rivo.Application.Finance.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.Application.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly IFinanceService _finance;
    private readonly IAnalyticsService _analytics;
    private readonly IDashboardRepository _repository;
    private readonly IDateTimeService _dateTimeService;

    public DashboardService(
        IApplicationDbContext context,
        IFinanceService finance,
        IAnalyticsService analytics,
        IDashboardRepository repository,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _finance = finance;
        _analytics = analytics;
        _repository = repository;
        _dateTimeService = dateTimeService;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var finance = await _finance.GetSummaryAsync(from, to, cancellationToken);

        var orders = await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.Status != OrderStatus.Voided)
            .ToListAsync(cancellationToken);

        var itemsSold = await _context.OrderItems.AsNoTracking()
            .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to && oi.Order.Status != OrderStatus.Voided)
            .SumAsync(oi => (int?)oi.Quantity, cancellationToken) ?? 0;

        var lowStock = await _analytics.GetLowStockAsync(cancellationToken);

        var ordersCount = orders.Count;
        var totalSales = orders.Sum(o => o.TotalAmount);

        return new DashboardOverviewDto
        {
            From = from,
            To = to,
            TotalSales = totalSales,
            OrdersCount = ordersCount,
            AverageCheck = ordersCount == 0 ? 0 : totalSales / ordersCount,
            TotalExpenses = finance.TotalExpenses,
            NetProfit = finance.NetProfit,
            ItemsSoldCount = itemsSold,
            LowStockCount = lowStock.Count,
        };
    }

    public async Task<DashboardDto> GetHomeOverviewAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var todayStart = _dateTimeService.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var yesterdayStart = todayStart.AddDays(-1);
        var weekStart = todayStart.AddDays(-6);

        var (todayTotal, todayCount) = await _repository.GetSalesSummaryAsync(tenantId, branchId, todayStart, tomorrowStart, cancellationToken);
        var (yesterdayTotal, yesterdayCount) = await _repository.GetSalesSummaryAsync(tenantId, branchId, yesterdayStart, todayStart, cancellationToken);
        var dailySales = await _repository.GetDailySalesAsync(tenantId, branchId, weekStart, tomorrowStart, cancellationToken);
        var topProducts = await _repository.GetTopProductsAsync(tenantId, branchId, todayStart, tomorrowStart, take: 5, cancellationToken);
        var (lowStockCount, lowStockWarehouseCount) = await _repository.GetLowStockSummaryAsync(tenantId, cancellationToken);

        var averageToday = todayCount > 0 ? todayTotal / todayCount : 0m;
        var averageYesterday = yesterdayCount > 0 ? yesterdayTotal / yesterdayCount : 0m;

        var salesByDate = dailySales.ToDictionary(x => x.Date, x => x.Total);
        var weeklySales = Enumerable.Range(0, 7)
            .Select(offset => DateOnly.FromDateTime(weekStart.AddDays(offset)))
            .Select(date => new DailySalesPointDto { Date = date, Total = salesByDate.GetValueOrDefault(date) })
            .ToList();

        return new DashboardDto
        {
            SalesToday = todayTotal,
            SalesChangePercent = PercentChange(todayTotal, yesterdayTotal),
            OrdersToday = todayCount,
            OrdersChangePercent = PercentChange(todayCount, yesterdayCount),
            AverageCheckToday = averageToday,
            AverageCheckChangePercent = PercentChange(averageToday, averageYesterday),
            LowStockProductCount = lowStockCount,
            LowStockWarehouseCount = lowStockWarehouseCount,
            WeeklySales = weeklySales,
            TopProducts = topProducts
                .Select(p => new TopProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    QuantitySold = p.Quantity,
                    Revenue = p.Revenue,
                })
                .ToList(),
        };
    }

    private static decimal? PercentChange(decimal current, decimal previous)
    {
        if (previous == 0m) return null;
        return Math.Round((current - previous) / previous * 100m, 1);
    }

    private static decimal? PercentChange(int current, int previous)
    {
        if (previous == 0) return null;
        return Math.Round((decimal)(current - previous) / previous * 100m, 1);
    }
}
