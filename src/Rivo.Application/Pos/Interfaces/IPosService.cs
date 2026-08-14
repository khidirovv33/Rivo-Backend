using Rivo.Application.Orders.Dtos;
using Rivo.Application.Pos.Dtos;

namespace Rivo.Application.Pos.Interfaces;

public interface IPosService
{
    /// <summary>Cart -> sale in one call: prices from the product catalog (never trusts client-sent prices), decrements stock (Dev2 contract), records payments, accrues loyalty.</summary>
    Task<OrderDto> CheckoutAsync(Guid tenantId, Guid cashierUserId, CheckoutRequestDto request, CancellationToken cancellationToken = default);

    Task<byte[]> GenerateReceiptPdfAsync(Guid tenantId, Guid orderId, CancellationToken cancellationToken = default);
}
