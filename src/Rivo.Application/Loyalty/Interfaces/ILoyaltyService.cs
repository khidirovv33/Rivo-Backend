using Rivo.Application.Loyalty.Dtos;

namespace Rivo.Application.Loyalty.Interfaces;

public interface ILoyaltyService
{
    Task<List<LoyaltyLevelDto>> GetLevelsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<LoyaltyLevelDto> CreateLevelAsync(Guid tenantId, CreateLoyaltyLevelRequestDto request, CancellationToken cancellationToken = default);
    Task<LoyaltyLevelDto> UpdateLevelAsync(Guid tenantId, Guid id, UpdateLoyaltyLevelRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteLevelAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);

    Task<LoyaltyCardDto> IssueCardAsync(Guid tenantId, IssueLoyaltyCardRequestDto request, CancellationToken cancellationToken = default);
    Task<LoyaltyCardDto?> GetCardByCustomerAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>Called by OrdersService after a completed sale: adds points and re-evaluates the customer's tier against spend thresholds.</summary>
    Task AccrueForSaleAsync(Guid tenantId, Guid customerId, decimal saleAmount, CancellationToken cancellationToken = default);
}
