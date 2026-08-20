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
