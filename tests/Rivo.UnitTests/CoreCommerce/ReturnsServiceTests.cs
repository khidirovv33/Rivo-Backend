using AutoMapper;
using Moq;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Mappings;
using Rivo.Application.Orders.Interfaces;
using Rivo.Application.Returns.Dtos;
using Rivo.Application.Returns.Interfaces;
using Rivo.Application.Returns.Services;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.UnitTests.CoreCommerce;

public class ReturnsServiceTests
{
    private readonly Mock<IReturnsRepository> _returnsRepository = new();
    private readonly Mock<IOrdersRepository> _ordersRepository = new();
    private readonly Mock<IStockAdjustmentService> _stockAdjustmentService = new();
    private readonly Mock<IFinanceIntegrationService> _financeIntegrationService = new();
    private readonly Mock<IApplicationDbContext> _dbContext = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _branchId = Guid.NewGuid();
    private readonly Guid _processedBy = Guid.NewGuid();

    private ReturnsService CreateSut() => new(
        _returnsRepository.Object,
        _ordersRepository.Object,
        _stockAdjustmentService.Object,
        _financeIntegrationService.Object,
        _dbContext.Object,
        _mapper);

    private (Order order, OrderItem item) SetUpOrderWithSingleItem(int quantity)
    {
        var order = new Order { Id = Guid.NewGuid(), TenantId = _tenantId, BranchId = _branchId };
        var item = new OrderItem { Id = Guid.NewGuid(), OrderId = order.Id, Quantity = quantity, LineTotal = quantity * 1000m, ProductId = Guid.NewGuid() };
        order.Items.Add(item);

        _ordersRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _ordersRepository.Setup(r => r.GetOrderItemByIdAsync(item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        return (order, item);
    }

    [Fact]
    public async Task CreateAsync_WhenReturnQuantityExceedsPurchased_ThrowsValidationAppException()
    {
        var (order, item) = SetUpOrderWithSingleItem(quantity: 2);
        _returnsRepository.Setup(r => r.GetReturnedQuantityForOrderItemAsync(item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var sut = CreateSut();
        var request = new CreateReturnRequestDto
        {
            OrderId = order.Id,
            Items = new List<CreateReturnItemRequestDto> { new() { OrderItemId = item.Id, Quantity = 3 } }
        };

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.CreateAsync(_tenantId, _processedBy, request));
    }

    [Fact]
    public async Task CreateAsync_WhenFullyReturned_SetsOrderStatusToRefundedAndIncreasesStock()
    {
        var (order, item) = SetUpOrderWithSingleItem(quantity: 2);
        _returnsRepository.Setup(r => r.GetReturnedQuantityForOrderItemAsync(item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var sut = CreateSut();
        var request = new CreateReturnRequestDto
        {
            OrderId = order.Id,
            Items = new List<CreateReturnItemRequestDto> { new() { OrderItemId = item.Id, Quantity = 2 } }
        };

        await sut.CreateAsync(_tenantId, _processedBy, request);

        Assert.Equal(OrderStatus.Refunded, order.Status);
        _stockAdjustmentService.Verify(s => s.IncreaseStockAsync(_tenantId, _branchId, item.ProductId, item.ProductVariationId, 2, It.IsAny<CancellationToken>()), Times.Once);
        _financeIntegrationService.Verify(f => f.RecordRefundAsync(_tenantId, It.IsAny<Guid>(), 2000m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenPartiallyReturned_SetsOrderStatusToPartiallyRefunded()
    {
        var (order, item) = SetUpOrderWithSingleItem(quantity: 4);
        _returnsRepository.Setup(r => r.GetReturnedQuantityForOrderItemAsync(item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var sut = CreateSut();
        var request = new CreateReturnRequestDto
        {
            OrderId = order.Id,
            Items = new List<CreateReturnItemRequestDto> { new() { OrderItemId = item.Id, Quantity = 1 } }
        };

        await sut.CreateAsync(_tenantId, _processedBy, request);

        Assert.Equal(OrderStatus.PartiallyRefunded, order.Status);
    }
}
