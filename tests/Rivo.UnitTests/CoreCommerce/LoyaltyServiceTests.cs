using AutoMapper;
using Moq;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Mappings;
using Rivo.Application.Customers.Interfaces;
using Rivo.Application.Loyalty.Dtos;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Application.Loyalty.Services;
using Rivo.Domain.Entities.Customers;
using Rivo.Domain.Entities.Loyalty;
using Rivo.Domain.Exceptions;

namespace Rivo.UnitTests.CoreCommerce;

public class LoyaltyServiceTests
{
    private readonly Mock<ILoyaltyRepository> _loyaltyRepository = new();
    private readonly Mock<ICustomersRepository> _customersRepository = new();
    private readonly Mock<IApplicationDbContext> _dbContext = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();

    private readonly Guid _tenantId = Guid.NewGuid();

    private LoyaltyService CreateSut() => new(
        _loyaltyRepository.Object,
        _customersRepository.Object,
        _dbContext.Object,
        _mapper);

    [Fact]
    public async Task AccrueForSaleAsync_AddsPointsAndUpdatesPurchaseTotals()
    {
        var customer = new Customer { TenantId = _tenantId, TotalPurchasesAmount = 0, LoyaltyPoints = 0, TotalOrdersCount = 0 };
        _customersRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _loyaltyRepository.Setup(r => r.GetCardByCustomerIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync((LoyaltyCard?)null);

        var sut = CreateSut();
        await sut.AccrueForSaleAsync(_tenantId, customer.Id, saleAmount: 100000m);

        Assert.Equal(100000m, customer.TotalPurchasesAmount);
        Assert.Equal(1, customer.TotalOrdersCount);
        Assert.Equal(10000, customer.LoyaltyPoints); // 0.1 point per currency unit
    }

    [Fact]
    public async Task AccrueForSaleAsync_WhenSpendCrossesThreshold_UpgradesCardToNextLevel()
    {
        var customer = new Customer { TenantId = _tenantId, TotalPurchasesAmount = 900000m };
        var currentLevel = Guid.NewGuid();
        var card = new LoyaltyCard { CustomerId = customer.Id, TenantId = _tenantId, LoyaltyLevelId = currentLevel };
        var nextLevel = new LoyaltyLevel { Id = Guid.NewGuid(), TenantId = _tenantId, Name = "VIP", MinimumSpend = 1000000m, DiscountPercentage = 15m };

        _customersRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _loyaltyRepository.Setup(r => r.GetCardByCustomerIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        _loyaltyRepository.Setup(r => r.GetHighestEligibleLevelAsync(_tenantId, 1000000m, It.IsAny<CancellationToken>())).ReturnsAsync(nextLevel);

        var sut = CreateSut();
        await sut.AccrueForSaleAsync(_tenantId, customer.Id, saleAmount: 100000m);

        Assert.Equal(nextLevel.Id, card.LoyaltyLevelId);
        _loyaltyRepository.Verify(r => r.UpdateCard(card), Times.Once);
    }

    [Fact]
    public async Task IssueCardAsync_WhenCustomerAlreadyHasCard_ThrowsValidationAppException()
    {
        var customer = new Customer { TenantId = _tenantId };
        _customersRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _loyaltyRepository.Setup(r => r.GetCardByCustomerIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoyaltyCard { CustomerId = customer.Id, TenantId = _tenantId });

        var sut = CreateSut();
        var request = new IssueLoyaltyCardRequestDto { CustomerId = customer.Id };

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.IssueCardAsync(_tenantId, request));
    }
}
