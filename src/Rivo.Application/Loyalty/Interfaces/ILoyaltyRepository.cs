using Rivo.Domain.Entities.Loyalty;

namespace Rivo.Application.Loyalty.Interfaces;

public interface ILoyaltyRepository
{
    Task<List<LoyaltyLevel>> GetLevelsByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<LoyaltyLevel?> GetLevelByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LoyaltyLevel?> GetHighestEligibleLevelAsync(Guid tenantId, decimal totalSpend, CancellationToken cancellationToken = default);
    Task AddLevelAsync(LoyaltyLevel level, CancellationToken cancellationToken = default);
    void UpdateLevel(LoyaltyLevel level);
    void RemoveLevel(LoyaltyLevel level);

    Task<LoyaltyCard?> GetCardByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LoyaltyCard?> GetCardByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task AddCardAsync(LoyaltyCard card, CancellationToken cancellationToken = default);
    void UpdateCard(LoyaltyCard card);
}
