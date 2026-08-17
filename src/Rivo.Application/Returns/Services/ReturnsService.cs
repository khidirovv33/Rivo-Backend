using AutoMapper;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Orders.Interfaces;
using Rivo.Application.Returns.Dtos;
using Rivo.Application.Returns.Interfaces;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Entities.Returns;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Returns.Services;

public class ReturnsService : IReturnsService
{
    private readonly IReturnsRepository _returnsRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IStockAdjustmentService _stockAdjustmentService;
    private readonly IFinanceIntegrationService _financeIntegrationService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ReturnsService(
        IReturnsRepository returnsRepository,
        IOrdersRepository ordersRepository,
        IStockAdjustmentService stockAdjustmentService,
        IFinanceIntegrationService financeIntegrationService,
        IApplicationDbContext dbContext,
        IMapper mapper)
    {
        _returnsRepository = returnsRepository;
        _ordersRepository = ordersRepository;
        _stockAdjustmentService = stockAdjustmentService;
        _financeIntegrationService = financeIntegrationService;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ReturnDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var returnEntity = await _returnsRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Returns.Return), id);

        if (returnEntity.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return _mapper.Map<ReturnDto>(returnEntity);
    }

    public async Task<PaginatedList<ReturnDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _returnsRepository.GetPagedAsync(
            tenantId, request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken);

        var dtos = items.Select(r => _mapper.Map<ReturnDto>(r)).ToList();
        return new PaginatedList<ReturnDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<ReturnDto> CreateAsync(Guid tenantId, Guid processedByUserId, CreateReturnRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.Items)] = new[] { "At least one item must be returned." }
            });
        }

        var order = await _ordersRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);
        if (order.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        var returnEntity = new Domain.Entities.Returns.Return
        {
            TenantId = tenantId,
            OrderId = order.Id,
            ProcessedByUserId = processedByUserId,
            Reason = request.Reason,
            CreatedBy = processedByUserId
        };

        decimal totalRefund = 0;
        var stockIncrements = new List<(Guid ProductId, Guid? VariationId, int Quantity)>();

        foreach (var itemRequest in request.Items)
        {
            var orderItem = await _ordersRepository.GetOrderItemByIdAsync(itemRequest.OrderItemId, cancellationToken)
                ?? throw new NotFoundException(nameof(OrderItem), itemRequest.OrderItemId);
            if (orderItem.OrderId != order.Id)
            {
                throw new NotFoundException(nameof(OrderItem), itemRequest.OrderItemId);
            }

            var alreadyReturned = await _returnsRepository.GetReturnedQuantityForOrderItemAsync(orderItem.Id, cancellationToken);
            if (itemRequest.Quantity <= 0 || alreadyReturned + itemRequest.Quantity > orderItem.Quantity)
            {
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    [nameof(itemRequest.Quantity)] = new[]
                    {
                        $"Cannot return {itemRequest.Quantity} unit(s): only {orderItem.Quantity - alreadyReturned} remain returnable for this line."
                    }
                });
            }

            var refundAmount = Math.Round(orderItem.LineTotal / orderItem.Quantity * itemRequest.Quantity, 2);
            totalRefund += refundAmount;

            returnEntity.Items.Add(new ReturnItem
            {
                OrderItemId = orderItem.Id,
                Quantity = itemRequest.Quantity,
                RefundAmount = refundAmount
            });

            stockIncrements.Add((orderItem.ProductId, orderItem.ProductVariationId, itemRequest.Quantity));
        }

        returnEntity.TotalRefundAmount = totalRefund;
        returnEntity.Status = ReturnStatus.Completed;

        await _returnsRepository.AddAsync(returnEntity, cancellationToken);

        // Queried before SaveChanges, so these totals reflect prior returns only; this return's own quantities are added in explicitly.
        var thisReturnByOrderItem = returnEntity.Items.ToDictionary(i => i.OrderItemId, i => i.Quantity);
        var totalOrderQuantity = order.Items.Sum(i => i.Quantity);
        var totalReturnedAfterThis = 0;
        foreach (var item in order.Items)
        {
            var previouslyReturned = await _returnsRepository.GetReturnedQuantityForOrderItemAsync(item.Id, cancellationToken);
            var thisReturn = thisReturnByOrderItem.GetValueOrDefault(item.Id);
            totalReturnedAfterThis += previouslyReturned + thisReturn;
        }
        order.Status = totalReturnedAfterThis >= totalOrderQuantity ? OrderStatus.Refunded : OrderStatus.PartiallyRefunded;
        order.UpdatedBy = processedByUserId;
        order.UpdatedAt = DateTime.UtcNow;
        _ordersRepository.Update(order);

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var (productId, variationId, quantity) in stockIncrements)
        {
            await _stockAdjustmentService.IncreaseStockAsync(tenantId, order.BranchId, productId, variationId, quantity, cancellationToken);
        }

        await _financeIntegrationService.RecordRefundAsync(tenantId, returnEntity.Id, totalRefund, cancellationToken);

        return _mapper.Map<ReturnDto>(returnEntity);
    }
}
