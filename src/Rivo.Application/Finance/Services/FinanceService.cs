using Microsoft.EntityFrameworkCore;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Finance.Dtos;
using Rivo.Application.Finance.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.Application.Finance.Services;

public class FinanceService : IFinanceService
{
    private readonly IApplicationDbContext _context;

    public FinanceService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinanceSummaryDto> GetSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var revenue = await _context.Incomes
            .Where(x => x.IncomeDate >= from && x.IncomeDate <= to)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;

        // COGS: cost of items actually sold in the period (Voided orders never happened commercially).
        var cogs = await _context.OrderItems
            .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to && oi.Order.Status != OrderStatus.Voided)
            .SumAsync(oi => (decimal?)(oi.Quantity * oi.Product.PurchasePrice), cancellationToken) ?? 0;

        var totalExpenses = await _context.Expenses
            .Where(x => x.ExpenseDate >= from && x.ExpenseDate <= to)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;

        var cashFlow = await _context.AccountTransactions
            .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
            .SumAsync(x => (decimal?)(x.Type == AccountTransactionType.Inflow ? x.Amount : -x.Amount), cancellationToken) ?? 0;

        var grossProfit = revenue - cogs;

        return new FinanceSummaryDto
        {
            From = from,
            To = to,
            Revenue = revenue,
            Cogs = cogs,
            GrossProfit = grossProfit,
            TotalExpenses = totalExpenses,
            NetProfit = grossProfit - totalExpenses,
            CashFlow = cashFlow,
        };
    }
}
