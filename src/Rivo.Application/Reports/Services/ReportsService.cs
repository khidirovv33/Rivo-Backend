using Microsoft.EntityFrameworkCore;
using Rivo.Application.Analytics.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Reports.Dtos;
using Rivo.Application.Reports.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.Application.Reports.Services;

public class ReportsService : IReportsService
{
    private readonly IApplicationDbContext _context;
    private readonly IAnalyticsService _analytics;
    private readonly IPdfExportService _pdf;
    private readonly IExcelExportService _excel;
    private readonly ICsvExportService _csv;

    public ReportsService(
        IApplicationDbContext context,
        IAnalyticsService analytics,
        IPdfExportService pdf,
        IExcelExportService excel,
        ICsvExportService csv)
    {
        _context = context;
        _analytics = analytics;
        _pdf = pdf;
        _excel = excel;
        _csv = csv;
    }

    public async Task<ReportTableDto> GetSalesReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new List<string>
            {
                o.OrderNumber,
                o.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                o.Branch.Name,
                o.CashierUser.FullName,
                o.Status.ToString(),
                o.TotalAmount.ToString("0.00"),
            })
            .ToListAsync(cancellationToken);

        return new ReportTableDto
        {
            Title = "Sales Report",
            Columns = ["Order #", "Date", "Branch", "Cashier", "Status", "Total"],
            Rows = rows,
        };
    }

    public async Task<ReportTableDto> GetInventoryReportAsync(CancellationToken cancellationToken = default)
    {
        var stocks = await _context.Stocks.AsNoTracking()
            .Where(s => s.SystemQuantity != 0 || s.ReservedQuantity != 0)
            .Select(s => new { Warehouse = s.Warehouse.Name, s.ProductId, s.SystemQuantity, s.ReservedQuantity })
            .ToListAsync(cancellationToken);

        var products = await _context.Products.AsNoTracking()
            .Where(p => stocks.Select(s => s.ProductId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => new { p.Name, p.Sku }, cancellationToken);

        var rows = stocks
            .OrderBy(x => x.Warehouse)
            .Select(x =>
            {
                var product = products.GetValueOrDefault(x.ProductId);
                return new List<string>
                {
                    x.Warehouse, product?.Name ?? "?", product?.Sku ?? "?",
                    x.SystemQuantity.ToString("0.###"),
                    x.ReservedQuantity.ToString("0.###"),
                    (x.SystemQuantity - x.ReservedQuantity).ToString("0.###"),
                };
            })
            .ToList();

        return new ReportTableDto
        {
            Title = "Inventory Report",
            Columns = ["Warehouse", "Product", "SKU", "System Qty", "Reserved", "Available"],
            Rows = rows,
        };
    }

    public async Task<ReportTableDto> GetFinancialReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var incomeRows = await _context.Incomes.AsNoTracking()
            .Where(x => x.IncomeDate >= from && x.IncomeDate <= to)
            .Select(x => new { Date = x.IncomeDate, Type = "Income:" + x.Type, x.Amount, x.Description })
            .ToListAsync(cancellationToken);

        var expenseRows = await _context.Expenses.AsNoTracking()
            .Where(x => x.ExpenseDate >= from && x.ExpenseDate <= to)
            .Select(x => new { Date = x.ExpenseDate, Type = "Expense:" + x.Category, Amount = -x.Amount, x.Description })
            .ToListAsync(cancellationToken);

        var rows = incomeRows
            .Select(x => new { x.Date, x.Type, x.Amount, x.Description })
            .Concat(expenseRows.Select(x => new { x.Date, x.Type, x.Amount, x.Description }))
            .OrderBy(x => x.Date)
            .Select(x => new List<string> { x.Date.ToString("yyyy-MM-dd HH:mm"), x.Type, x.Amount.ToString("0.00"), x.Description ?? string.Empty })
            .ToList();

        return new ReportTableDto
        {
            Title = "Financial Report",
            Columns = ["Date", "Type", "Amount", "Description"],
            Rows = rows,
        };
    }

    public async Task<ReportTableDto> GetProfitReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var trend = await _analytics.GetSalesTrendAsync(from, to, cancellationToken);

        var rows = trend
            .Select(x => new List<string>
            {
                x.Date.ToString("yyyy-MM-dd"),
                x.Sales.ToString("0.00"),
                (x.Sales - x.Profit).ToString("0.00"),
                x.Profit.ToString("0.00"),
                x.OrdersCount.ToString(),
            })
            .ToList();

        return new ReportTableDto
        {
            Title = "Profit Report",
            Columns = ["Date", "Revenue", "COGS", "Gross Profit", "Orders"],
            Rows = rows,
        };
    }

    public async Task<ReportTableDto> GetPurchaseReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Purchases.AsNoTracking()
            .Where(x => x.PurchaseDate >= from && x.PurchaseDate <= to)
            .Select(x => new
            {
                x.PurchaseDate,
                Supplier = _context.Suppliers.Where(s => s.Id == x.SupplierId).Select(s => s.Name).FirstOrDefault() ?? "?",
                x.TotalAmount,
                x.PaidAmount,
                x.OutstandingAmount,
            })
            .OrderBy(x => x.PurchaseDate)
            .Select(x => new List<string>
            {
                x.PurchaseDate.ToString("yyyy-MM-dd"), x.Supplier,
                x.TotalAmount.ToString("0.00"), x.PaidAmount.ToString("0.00"), x.OutstandingAmount.ToString("0.00"),
            })
            .ToListAsync(cancellationToken);

        return new ReportTableDto
        {
            Title = "Purchase Report",
            Columns = ["Date", "Supplier", "Total", "Paid", "Outstanding"],
            Rows = rows,
        };
    }

    public async Task<ReportTableDto> GetEmployeeReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var stats = await _analytics.GetEmployeeStatsAsync(from, to, cancellationToken);

        var rows = stats
            .Select(x => new List<string>
            {
                x.UserName, x.OrdersCount.ToString(), x.TotalSales.ToString("0.00"), x.AverageCheck.ToString("0.00"),
            })
            .ToList();

        return new ReportTableDto
        {
            Title = "Employee Report",
            Columns = ["Employee", "Orders", "Total Sales", "Average Check"],
            Rows = rows,
        };
    }

    public async Task<ReportTableDto> GetAuditReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var rows = await _context.AuditLogs.AsNoTracking()
            .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new List<string>
            {
                x.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                x.UserId.HasValue ? x.UserId.Value.ToString() : "system",
                x.Action, x.EntityName, x.EntityId, x.IpAddress ?? string.Empty,
            })
            .Take(5000)
            .ToListAsync(cancellationToken);

        return new ReportTableDto
        {
            Title = "Audit Report",
            Columns = ["Date", "User", "Action", "Entity", "Entity Id", "IP"],
            Rows = rows,
        };
    }

    public async Task<ReportTableDto> GetInventoryDifferenceReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Inventories.AsNoTracking()
            .Where(i => i.Status == InventoryStatus.Approved && i.ApprovedAt >= from && i.ApprovedAt <= to)
            .SelectMany(i => i.Items.Where(item => item.ActualQuantity != item.SystemQuantity).Select(item => new
            {
                i.InventoryNumber,
                i.ApprovedAt,
                i.WarehouseId,
                item.ProductId,
                item.SystemQuantity,
                item.ActualQuantity,
                item.Difference,
                item.DifferenceCost,
            }))
            .OrderByDescending(x => x.ApprovedAt)
            .ToListAsync(cancellationToken);

        var productNames = await _context.Products.AsNoTracking()
            .Where(p => rows.Select(r => r.ProductId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var warehouseNames = await _context.Warehouses.AsNoTracking()
            .Where(w => rows.Select(r => r.WarehouseId).Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.Name, cancellationToken);

        return new ReportTableDto
        {
            Title = "Inventory Difference Report",
            Columns = ["Inventory #", "Approved", "Warehouse", "Product", "System", "Actual", "Difference", "Diff. Cost"],
            Rows = rows.Select(x => new List<string>
            {
                x.InventoryNumber,
                x.ApprovedAt?.ToString("yyyy-MM-dd") ?? string.Empty,
                warehouseNames.GetValueOrDefault(x.WarehouseId, "?"),
                productNames.GetValueOrDefault(x.ProductId, "?"),
                x.SystemQuantity.ToString("0.###"),
                x.ActualQuantity.ToString("0.###"),
                x.Difference.ToString("0.###"),
                x.DifferenceCost.ToString("0.00"),
            }).ToList(),
        };
    }

    public byte[] Export(ReportTableDto report, ReportExportFormat format) => format switch
    {
        ReportExportFormat.Pdf => _pdf.GenerateTableReportPdf(report.Title, report.Columns, report.Rows),
        ReportExportFormat.Excel => _excel.Export(report.Title, report.Columns, report.Rows),
        ReportExportFormat.Csv => _csv.Export(report.Columns, report.Rows),
        _ => throw new ArgumentOutOfRangeException(nameof(format)),
    };
}
