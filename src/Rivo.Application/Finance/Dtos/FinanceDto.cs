namespace Rivo.Application.Finance.Dtos;

/// <summary>Раздел 12 ТЗ: Revenue - COGS = Gross Profit; Gross Profit - Expenses = Net Profit.</summary>
public class FinanceSummaryDto
{
    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public decimal Revenue { get; set; }

    public decimal Cogs { get; set; }

    public decimal GrossProfit { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal NetProfit { get; set; }

    /// <summary>Чистое движение денежных средств за период (по всем счетам).</summary>
    public decimal CashFlow { get; set; }
}

public class FinanceRequestDto
{
    public DateTime From { get; set; }

    public DateTime To { get; set; }
}
