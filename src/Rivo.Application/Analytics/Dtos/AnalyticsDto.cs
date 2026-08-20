namespace Rivo.Application.Analytics.Dtos;

/// <summary>Раздел 14 ТЗ. Period — "day"|"week"|"month"|"year"; from/to переопределяют его для произвольного периода.</summary>
public class AnalyticsRequestDto
{
    public DateTime From { get; set; }

    public DateTime To { get; set; }
}

public class SalesTrendPointDto
{
    public DateOnly Date { get; set; }

    public decimal Sales { get; set; }

    public decimal Profit { get; set; }

    public int OrdersCount { get; set; }
}

public class ProductRankingDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal QuantitySold { get; set; }

    public decimal Revenue { get; set; }

    public decimal Profit { get; set; }
}

public class SlowMovingProductDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal QuantitySoldInPeriod { get; set; }

    public decimal CurrentStock { get; set; }
}

public class LowStockItemDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal CurrentStock { get; set; }

    public int MinimumStock { get; set; }
}

public class EmployeeStatDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = null!;

    public int OrdersCount { get; set; }

    public decimal TotalSales { get; set; }

    public decimal AverageCheck { get; set; }
}

public class BranchComparisonDto
{
    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = null!;

    public int OrdersCount { get; set; }

    public decimal TotalSales { get; set; }

    public decimal TotalProfit { get; set; }
}
