using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Orders.Dtos;

namespace Rivo.Infrastructure.ExternalServices;

public class PdfExportService : IPdfExportService
{
    public byte[] GenerateReceiptPdf(OrderDto order, string storeName, string? customerName)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A6);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(storeName).FontSize(14).Bold();
                    col.Item().Text($"Receipt #{order.OrderNumber}");
                    col.Item().Text($"{order.CreatedAt:yyyy-MM-dd HH:mm}");
                    if (!string.IsNullOrWhiteSpace(customerName))
                    {
                        col.Item().Text($"Customer: {customerName}");
                    }
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Item").Bold();
                        header.Cell().Text("Qty").Bold();
                        header.Cell().Text("Total").Bold();
                    });

                    foreach (var item in order.Items)
                    {
                        table.Cell().Text(item.ProductName);
                        table.Cell().Text(item.Quantity.ToString());
                        table.Cell().Text(item.LineTotal.ToString("0.00"));
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f);
                    col.Item().Text($"Subtotal: {order.SubTotal:0.00}");
                    col.Item().Text($"Discount: {order.DiscountAmount:0.00}");
                    col.Item().Text($"Tax: {order.TaxAmount:0.00}");
                    col.Item().Text($"Total: {order.TotalAmount:0.00}").Bold();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateTableReportPdf(string title, IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text(title).FontSize(16).Bold();
                    col.Item().Text($"{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        foreach (var _ in columns)
                        {
                            cols.RelativeColumn();
                        }
                    });

                    table.Header(header =>
                    {
                        foreach (var column in columns)
                        {
                            header.Cell().Text(column).Bold();
                        }
                    });

                    foreach (var row in rows)
                    {
                        foreach (var cell in row)
                        {
                            table.Cell().Text(cell);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
