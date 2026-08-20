using Rivo.Application.Reports.Dtos;

namespace Rivo.Application.Reports.Interfaces;

/// <summary>Раздел 15 ТЗ — восемь отчётов, каждый из них экспортируется в PDF/Excel/CSV через ExportAsync.</summary>
public interface IReportsService
{
    Task<ReportTableDto> GetSalesReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<ReportTableDto> GetInventoryReportAsync(CancellationToken cancellationToken = default);

    Task<ReportTableDto> GetFinancialReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<ReportTableDto> GetProfitReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<ReportTableDto> GetPurchaseReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<ReportTableDto> GetEmployeeReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<ReportTableDto> GetAuditReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<ReportTableDto> GetInventoryDifferenceReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    byte[] Export(ReportTableDto report, ReportExportFormat format);
}
