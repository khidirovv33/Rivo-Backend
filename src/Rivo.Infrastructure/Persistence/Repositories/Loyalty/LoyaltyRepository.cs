using Microsoft.EntityFrameworkCore;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Domain.Entities.Loyalty;

namespace Rivo.Infrastructure.Persistence.Repositories.Loyalty;

public class LoyaltyRepository : ILoyaltyRepository
{
    private readonly ApplicationDbContext _context;

    public LoyaltyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<LoyaltyLevel>> GetLevelsByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.LoyaltyLevels.Where(l => l.TenantId == tenantId).OrderBy(l => l.MinimumSpend).ToListAsync(cancellationToken);

    public Task<LoyaltyLevel?> GetLevelByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.LoyaltyLevels.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<LoyaltyLevel?> GetHighestEligibleLevelAsync(Guid tenantId, decimal totalSpend, CancellationToken cancellationToken = default) =>
        _context.LoyaltyLevels
            .Where(l => l.TenantId == tenantId && l.MinimumSpend <= totalSpend)
            .OrderByDescending(l => l.MinimumSpend)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddLevelAsync(LoyaltyLevel level, CancellationToken cancellationToken = default) =>
        await _context.LoyaltyLevels.AddAsync(level, cancellationToken);

    public void UpdateLevel(LoyaltyLevel level) => _context.LoyaltyLevels.Update(level);

    public void RemoveLevel(LoyaltyLevel level) => _context.LoyaltyLevels.Remove(level);

    public Task<LoyaltyCard?> GetCardByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.LoyaltyCards.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<LoyaltyCard?> GetCardByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        _context.LoyaltyCards.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

    public async Task AddCardAsync(LoyaltyCard card, CancellationToken cancellationToken = default) =>
        await _context.LoyaltyCards.AddAsync(card, cancellationToken);

    public void UpdateCard(LoyaltyCard card) => _context.LoyaltyCards.Update(card);
}
