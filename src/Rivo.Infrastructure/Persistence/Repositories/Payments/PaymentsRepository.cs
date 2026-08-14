using Microsoft.EntityFrameworkCore;
using Rivo.Application.Payments.Interfaces;
using Rivo.Domain.Entities.Payments;

namespace Rivo.Infrastructure.Persistence.Repositories.Payments;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        _context.Payments.Where(p => p.OrderId == orderId).ToListAsync(cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>
        await _context.Payments.AddAsync(payment, cancellationToken);
}
