using Rivo.Application.Payments.Dtos;

namespace Rivo.Application.Payments.Interfaces;

/// <summary>Read-side only. Payments are created as part of IPosService.CheckoutAsync.</summary>
public interface IPaymentsService
{
    Task<List<PaymentDto>> GetByOrderIdAsync(Guid tenantId, Guid orderId, CancellationToken cancellationToken = default);
}
