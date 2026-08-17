using AutoMapper;
using Rivo.Application.Common.Models;
using Rivo.Application.Orders.Dtos;
using Rivo.Application.Orders.Interfaces;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Orders.Services;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;

    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper)
    {
        _ordersRepository = ordersRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _ordersRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        if (order.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<PaginatedList<OrderDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _ordersRepository.GetPagedAsync(
            tenantId, request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken);

        var dtos = items.Select(o => _mapper.Map<OrderDto>(o)).ToList();
        return new PaginatedList<OrderDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
