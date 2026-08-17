using Rivo.Application.Orders.Dtos;

namespace Rivo.Application.Common.Interfaces;

public interface IPdfExportService
{
    byte[] GenerateReceiptPdf(OrderDto order, string storeName, string? customerName);
}
