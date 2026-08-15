using AutoMapper;
using Moq;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Mappings;
using Rivo.Application.Loyalty.Dtos;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Application.Orders.Interfaces;
using Rivo.Application.Payments.Dtos;
using Rivo.Application.Payments.Interfaces;
using Rivo.Application.Pos.Dtos;
using Rivo.Application.Pos.Services;
using Rivo.Application.Products.Interfaces;
using Rivo.Application.Stores.Interfaces;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Entities.Products;
using Rivo.Domain.Entities.Stores;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.UnitTests.CoreCommerce;

public class PosServiceTests
{
    private readonly Mock<IProductsRepository> _productsRepository = new();
    private readonly Mock<IStoresRepository> _storesRepository = new();
    private readonly Mock<IOrdersRepository> _ordersRepository = new();
    private readonly Mock<IPaymentsRepository> _paymentsRepository = new();
    private readonly Mock<IStockAdjustmentService> _stockAdjustmentService = new();
    private readonly Mock<IFinanceIntegrationService> _financeIntegrationService = new();
    private readonly Mock<ILoyaltyService> _loyaltyService = new();
    private readonly Mock<IApplicationDbContext> _dbContext = new();
    private readonly Mock<IPdfExportService> _pdfExportService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _storeId = Guid.NewGuid();
    private readonly Guid _branchId = Guid.NewGuid();
    private readonly Guid _cashierId = Guid.NewGuid();

    private PosService CreateSut() => new(
        _productsRepository.Object,
        _storesRepository.Object,
        _ordersRepository.Object,
        _paymentsRepository.Object,
        _stockAdjustmentService.Object,
        _financeIntegrationService.Object,
        _loyaltyService.Object,
        _dbContext.Object,
        _pdfExportService.Object,
        _emailService.Object,
        _mapper);

    private void SetUpStoreAndBranch()
    {
        _storesRepository.Setup(r => r.GetByIdAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Store { Id = _storeId, TenantId = _tenantId, Name = "Чиланзар" });
        _storesRepository.Setup(r => r.GetBranchByIdAsync(_branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = _branchId, TenantId = _tenantId, StoreId = _storeId });
    }

    private Product SetUpProduct(decimal sellingPrice, decimal taxRate)
    {
        var product = new Product { TenantId = _tenantId, SellingPrice = sellingPrice, TaxRate = taxRate, Name = "Coca-Cola 1.5л" };
        _productsRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        return product;
    }

    /// <summary>PosService re-fetches the just-created order by its freshly generated Id, so the stub must
    /// resolve lazily against whatever AddAsync captured rather than a Guid known ahead of time.</summary>
    private void SetUpPersistedOrderRefetch(Func<Order?> capturedOrder)
    {
        _ordersRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(capturedOrder);
    }

    [Fact]
    public async Task CheckoutAsync_WithMatchingPayments_ComputesTotalsAndDecrementsStock()
    {
        SetUpStoreAndBranch();
        var product = SetUpProduct(sellingPrice: 10000m, taxRate: 10m);

        Order? addedOrder = null;
        _ordersRepository.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => addedOrder = o)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var request = new CheckoutRequestDto
        {
            StoreId = _storeId,
            BranchId = _branchId,
            Items = new List<CheckoutItemRequestDto> { new() { ProductId = product.Id, Quantity = 2 } },
            // 2 * 10000 = 20000 subtotal, +10% tax = 22000 total
            Payments = new List<CreatePaymentRequestDto> { new() { Method = PaymentMethod.Cash, Amount = 22000m } }
        };

        SetUpPersistedOrderRefetch(() => addedOrder);

        await sut.CheckoutAsync(_tenantId, _cashierId, request);

        Assert.NotNull(addedOrder);
        Assert.Equal(20000m, addedOrder!.SubTotal);
        Assert.Equal(2000m, addedOrder.TaxAmount);
        Assert.Equal(22000m, addedOrder.TotalAmount);

        _stockAdjustmentService.Verify(s => s.DecreaseStockAsync(_tenantId, _branchId, product.Id, null, 2, It.IsAny<CancellationToken>()), Times.Once);
        _financeIntegrationService.Verify(f => f.RecordSaleAsync(_tenantId, addedOrder.Id, 22000m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_WhenPaymentsDoNotMatchTotal_ThrowsValidationAppException()
    {
        SetUpStoreAndBranch();
        var product = SetUpProduct(sellingPrice: 10000m, taxRate: 0m);

        var sut = CreateSut();
        var request = new CheckoutRequestDto
        {
            StoreId = _storeId,
            BranchId = _branchId,
            Items = new List<CheckoutItemRequestDto> { new() { ProductId = product.Id, Quantity = 1 } },
            Payments = new List<CreatePaymentRequestDto> { new() { Method = PaymentMethod.Cash, Amount = 500m } } // should be 10000
        };

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.CheckoutAsync(_tenantId, _cashierId, request));
        _ordersRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_WithLoyaltyCard_AppliesLevelDiscountToTotal()
    {
        SetUpStoreAndBranch();
        var product = SetUpProduct(sellingPrice: 10000m, taxRate: 0m);
        var customerId = Guid.NewGuid();

        _loyaltyService.Setup(l => l.GetCardByCustomerAsync(_tenantId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoyaltyCardDto { CustomerId = customerId, LoyaltyLevelDiscountPercentage = 10m });

        Order? addedOrder = null;
        _ordersRepository.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => addedOrder = o)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var request = new CheckoutRequestDto
        {
            StoreId = _storeId,
            BranchId = _branchId,
            CustomerId = customerId,
            Items = new List<CheckoutItemRequestDto> { new() { ProductId = product.Id, Quantity = 1 } },
            // 10000 subtotal - 10% loyalty discount = 9000 total (no tax)
            Payments = new List<CreatePaymentRequestDto> { new() { Method = PaymentMethod.Cash, Amount = 9000m } }
        };

        SetUpPersistedOrderRefetch(() => addedOrder);

        await sut.CheckoutAsync(_tenantId, _cashierId, request);

        Assert.NotNull(addedOrder);
        Assert.Equal(1000m, addedOrder!.DiscountAmount);
        Assert.Equal(9000m, addedOrder.TotalAmount);
    }
}
