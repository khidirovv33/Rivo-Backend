using AutoMapper;
using Rivo.Application.Orders.Interfaces;
using Rivo.Application.Payments.Dtos;
using Rivo.Application.Payments.Interfaces;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Payments.Services;

public class PaymentsService : IPaymentsService
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;

    public PaymentsService(IPaymentsRepository paymentsRepository, IOrdersRepository ordersRepository, IMapper mapper)
    {
        _paymentsRepository = paymentsRepository;
        _ordersRepository = ordersRepository;
        _mapper = mapper;
    }

    public async Task<List<PaymentDto>> GetByOrderIdAsync(Guid tenantId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _ordersRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);

        if (order.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        var payments = await _paymentsRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return payments.Select(p => _mapper.Map<PaymentDto>(p)).ToList();
    }
}
