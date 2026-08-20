using Rivo.Application.Orders.Dtos;

namespace Rivo.Application.Common.Interfaces;

public interface IPdfExportService
{
    byte[] GenerateReceiptPdf(OrderDto order, string storeName, string? customerName);

    /// <summary>Generic tabular report export (§15 ТЗ), shared by all of Dev3's Reports.</summary>
    byte[] GenerateTableReportPdf(string title, IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows);
}
