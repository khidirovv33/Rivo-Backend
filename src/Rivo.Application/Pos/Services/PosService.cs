using AutoMapper;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Application.Orders.Dtos;
using Rivo.Application.Orders.Interfaces;
using Rivo.Application.Payments.Interfaces;
using Rivo.Application.Pos.Dtos;
using Rivo.Application.Pos.Interfaces;
using Rivo.Application.Products.Interfaces;
using Rivo.Application.Stores.Interfaces;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Entities.Payments;
using Rivo.Domain.Entities.Products;
using Rivo.Domain.Entities.Stores;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Pos.Services;

public class PosService : IPosService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IStoresRepository _storesRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IStockAdjustmentService _stockAdjustmentService;
    private readonly IFinanceIntegrationService _financeIntegrationService;
    private readonly ILoyaltyService _loyaltyService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IPdfExportService _pdfExportService;
    private readonly IMapper _mapper;

    public PosService(
        IProductsRepository productsRepository,
        IStoresRepository storesRepository,
        IOrdersRepository ordersRepository,
        IPaymentsRepository paymentsRepository,
        IStockAdjustmentService stockAdjustmentService,
        IFinanceIntegrationService financeIntegrationService,
        ILoyaltyService loyaltyService,
        IApplicationDbContext dbContext,
        IPdfExportService pdfExportService,
        IMapper mapper)
    {
        _productsRepository = productsRepository;
        _storesRepository = storesRepository;
        _ordersRepository = ordersRepository;
        _paymentsRepository = paymentsRepository;
        _stockAdjustmentService = stockAdjustmentService;
        _financeIntegrationService = financeIntegrationService;
        _loyaltyService = loyaltyService;
        _dbContext = dbContext;
        _pdfExportService = pdfExportService;
        _mapper = mapper;
    }

    public async Task<OrderDto> CheckoutAsync(Guid tenantId, Guid cashierUserId, CheckoutRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.Items)] = new[] { "The cart must contain at least one item." }
            });
        }

        var store = await _storesRepository.GetByIdAsync(request.StoreId, cancellationToken)
            ?? throw new NotFoundException(nameof(Store), request.StoreId);
        if (store.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        var branch = await _storesRepository.GetBranchByIdAsync(request.BranchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), request.BranchId);
        if (branch.TenantId != tenantId || branch.StoreId != request.StoreId)
        {
            throw new TenantMismatchException();
        }

        var order = new Order
        {
            TenantId = tenantId,
            StoreId = request.StoreId,
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            CashierUserId = cashierUserId,
            OrderNumber = $"POS-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            CreatedBy = cashierUserId
        };

        decimal subTotal = 0, discountTotal = request.OrderDiscountAmount, taxTotal = 0;
        var stockDecrements = new List<(Guid ProductId, Guid? VariationId, int Quantity)>();

        foreach (var itemRequest in request.Items)
        {
            if (itemRequest.Quantity <= 0)
            {
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    [nameof(itemRequest.Quantity)] = new[] { "Quantity must be greater than zero." }
                });
            }

            var product = await _productsRepository.GetByIdAsync(itemRequest.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), itemRequest.ProductId);
            if (product.TenantId != tenantId)
            {
                throw new TenantMismatchException();
            }

            decimal priceAdjustment = 0;
            if (itemRequest.ProductVariationId.HasValue)
            {
                var variation = await _productsRepository.GetVariationByIdAsync(itemRequest.ProductVariationId.Value, cancellationToken)
                    ?? throw new NotFoundException(nameof(ProductVariation), itemRequest.ProductVariationId.Value);
                if (variation.ProductId != product.Id)
                {
                    throw new NotFoundException(nameof(ProductVariation), itemRequest.ProductVariationId.Value);
                }
                priceAdjustment = variation.PriceAdjustment;
            }

            var unitPrice = product.SellingPrice + priceAdjustment;
            var lineSubtotal = (unitPrice * itemRequest.Quantity) - itemRequest.DiscountAmount;
            var lineTax = Math.Round(lineSubtotal * product.TaxRate / 100m, 2);

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductVariationId = itemRequest.ProductVariationId,
                Quantity = itemRequest.Quantity,
                UnitPrice = unitPrice,
                DiscountAmount = itemRequest.DiscountAmount,
                TaxAmount = lineTax,
                LineTotal = lineSubtotal + lineTax
            });

            subTotal += unitPrice * itemRequest.Quantity;
            discountTotal += itemRequest.DiscountAmount;
            taxTotal += lineTax;

            stockDecrements.Add((product.Id, itemRequest.ProductVariationId, itemRequest.Quantity));
        }

        order.SubTotal = subTotal;
        order.DiscountAmount = discountTotal;
        order.TaxAmount = taxTotal;
        order.TotalAmount = subTotal - discountTotal + taxTotal;

        var paymentsTotal = request.Payments.Sum(p => p.Amount);
        if (Math.Abs(paymentsTotal - order.TotalAmount) > 0.01m)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.Payments)] = new[] { $"Payments total ({paymentsTotal}) must match the order total ({order.TotalAmount})." }
            });
        }

        await _ordersRepository.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var (productId, variationId, quantity) in stockDecrements)
        {
            await _stockAdjustmentService.DecreaseStockAsync(tenantId, request.BranchId, productId, variationId, quantity, cancellationToken);
        }

        foreach (var paymentRequest in request.Payments)
        {
            await _paymentsRepository.AddAsync(new Payment
            {
                TenantId = tenantId,
                OrderId = order.Id,
                Method = paymentRequest.Method,
                Amount = paymentRequest.Amount
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _financeIntegrationService.RecordSaleAsync(tenantId, order.Id, order.TotalAmount, cancellationToken);

        if (request.CustomerId.HasValue)
        {
            await _loyaltyService.AccrueForSaleAsync(tenantId, request.CustomerId.Value, order.TotalAmount, cancellationToken);
        }

        var persistedOrder = await _ordersRepository.GetByIdAsync(order.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), order.Id);

        return _mapper.Map<OrderDto>(persistedOrder);
    }

    public async Task<byte[]> GenerateReceiptPdfAsync(Guid tenantId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _ordersRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);
        if (order.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        var store = await _storesRepository.GetByIdAsync(order.StoreId, cancellationToken);
        var orderDto = _mapper.Map<OrderDto>(order);

        return _pdfExportService.GenerateReceiptPdf(orderDto, store?.Name ?? "Rivo", order.Customer?.FullName);
    }
}
