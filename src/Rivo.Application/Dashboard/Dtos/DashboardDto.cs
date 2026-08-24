namespace Rivo.Application.Dashboard.Dtos;

/// <summary>Раздел 14 ТЗ: продажи, расходы, прибыль, заказы, средний чек, низкие остатки.</summary>
public class DashboardOverviewDto
{
    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public decimal TotalSales { get; set; }

    public int OrdersCount { get; set; }

    public decimal AverageCheck { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal NetProfit { get; set; }

    public int ItemsSoldCount { get; set; }

    public int LowStockCount { get; set; }
}

/// <summary>Сводка "сегодня + неделя" для главного экрана (Обзор) — отдельно от финансового DashboardOverviewDto.</summary>
public class DashboardDto
{
    public decimal SalesToday { get; set; }
    public decimal? SalesChangePercent { get; set; }

    public int OrdersToday { get; set; }
    public decimal? OrdersChangePercent { get; set; }

    public decimal AverageCheckToday { get; set; }
    public decimal? AverageCheckChangePercent { get; set; }

    public int LowStockProductCount { get; set; }
    public int LowStockWarehouseCount { get; set; }

    public List<DailySalesPointDto> WeeklySales { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class DailySalesPointDto
{
    public DateOnly Date { get; set; }
    public decimal Total { get; set; }
}

public class TopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
