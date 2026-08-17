using Rivo.Domain.Entities.Payments;

namespace Rivo.Application.Payments.Interfaces;

public interface IPaymentsRepository
{
    Task<List<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
