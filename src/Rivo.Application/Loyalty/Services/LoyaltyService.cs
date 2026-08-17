using AutoMapper;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Customers.Interfaces;
using Rivo.Application.Loyalty.Dtos;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Domain.Entities.Customers;
using Rivo.Domain.Entities.Loyalty;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Loyalty.Services;

public class LoyaltyService : ILoyaltyService
{
    /// <summary>Simple accrual rule: 1 loyalty point per 10 currency units spent. Tune per-tenant in a later iteration (Settings module).</summary>
    private const decimal PointsPerCurrencyUnit = 0.1m;

    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly ICustomersRepository _customersRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public LoyaltyService(
        ILoyaltyRepository loyaltyRepository,
        ICustomersRepository customersRepository,
        IApplicationDbContext dbContext,
        IMapper mapper)
    {
        _loyaltyRepository = loyaltyRepository;
        _customersRepository = customersRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<List<LoyaltyLevelDto>> GetLevelsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var levels = await _loyaltyRepository.GetLevelsByTenantAsync(tenantId, cancellationToken);
        return levels.Select(l => _mapper.Map<LoyaltyLevelDto>(l)).ToList();
    }

    public async Task<LoyaltyLevelDto> CreateLevelAsync(Guid tenantId, CreateLoyaltyLevelRequestDto request, CancellationToken cancellationToken = default)
    {
        var level = new LoyaltyLevel
        {
            TenantId = tenantId,
            Name = request.Name,
            MinimumSpend = request.MinimumSpend,
            DiscountPercentage = request.DiscountPercentage
        };

        await _loyaltyRepository.AddLevelAsync(level, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LoyaltyLevelDto>(level);
    }

    public async Task<LoyaltyLevelDto> UpdateLevelAsync(Guid tenantId, Guid id, UpdateLoyaltyLevelRequestDto request, CancellationToken cancellationToken = default)
    {
        var level = await GetTenantLevelOrThrowAsync(tenantId, id, cancellationToken);

        level.Name = request.Name;
        level.MinimumSpend = request.MinimumSpend;
        level.DiscountPercentage = request.DiscountPercentage;
        level.UpdatedAt = DateTime.UtcNow;

        _loyaltyRepository.UpdateLevel(level);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LoyaltyLevelDto>(level);
    }

    public async Task DeleteLevelAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var level = await GetTenantLevelOrThrowAsync(tenantId, id, cancellationToken);
        _loyaltyRepository.RemoveLevel(level);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<LoyaltyCardDto> IssueCardAsync(Guid tenantId, IssueLoyaltyCardRequestDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _customersRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        if (customer.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        if (await _loyaltyRepository.GetCardByCustomerIdAsync(request.CustomerId, cancellationToken) is not null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.CustomerId)] = new[] { "This customer already has a loyalty card." }
            });
        }

        var card = new LoyaltyCard
        {
            TenantId = tenantId,
            CustomerId = request.CustomerId,
            CardNumber = GenerateCardNumber(),
            LoyaltyLevelId = request.LoyaltyLevelId
        };

        await _loyaltyRepository.AddCardAsync(card, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await MapCardAsync(card, cancellationToken);
    }

    public async Task<LoyaltyCardDto?> GetCardByCustomerAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var card = await _loyaltyRepository.GetCardByCustomerIdAsync(customerId, cancellationToken);
        if (card is null || card.TenantId != tenantId)
        {
            return null;
        }

        return await MapCardAsync(card, cancellationToken);
    }

    public async Task AccrueForSaleAsync(Guid tenantId, Guid customerId, decimal saleAmount, CancellationToken cancellationToken = default)
    {
        var customer = await _customersRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null || customer.TenantId != tenantId)
        {
            return;
        }

        customer.TotalPurchasesAmount += saleAmount;
        customer.TotalOrdersCount += 1;
        customer.LoyaltyPoints += (int)Math.Floor(saleAmount * PointsPerCurrencyUnit);
        _customersRepository.Update(customer);

        var card = await _loyaltyRepository.GetCardByCustomerIdAsync(customerId, cancellationToken);
        if (card is not null)
        {
            var eligibleLevel = await _loyaltyRepository.GetHighestEligibleLevelAsync(tenantId, customer.TotalPurchasesAmount, cancellationToken);
            if (eligibleLevel is not null && eligibleLevel.Id != card.LoyaltyLevelId)
            {
                card.LoyaltyLevelId = eligibleLevel.Id;
                _loyaltyRepository.UpdateCard(card);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<LoyaltyCardDto> MapCardAsync(LoyaltyCard card, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<LoyaltyCardDto>(card);
        if (card.LoyaltyLevelId.HasValue)
        {
            var level = await _loyaltyRepository.GetLevelByIdAsync(card.LoyaltyLevelId.Value, cancellationToken);
            if (level is not null)
            {
                dto.LoyaltyLevelName = level.Name;
                dto.LoyaltyLevelDiscountPercentage = level.DiscountPercentage;
            }
        }

        return dto;
    }

    private async Task<LoyaltyLevel> GetTenantLevelOrThrowAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var level = await _loyaltyRepository.GetLevelByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(LoyaltyLevel), id);

        if (level.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return level;
    }

    private static string GenerateCardNumber() =>
        DateTime.UtcNow.Ticks.ToString()[^10..];
}
