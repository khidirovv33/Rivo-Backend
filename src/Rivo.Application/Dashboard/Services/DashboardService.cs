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

    public DashboardService(IApplicationDbContext context, IFinanceService finance, IAnalyticsService analytics)
    {
        _context = context;
        _finance = finance;
        _analytics = analytics;
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
}
