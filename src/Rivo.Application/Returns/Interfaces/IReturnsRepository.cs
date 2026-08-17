using Rivo.Domain.Entities.Returns;

namespace Rivo.Application.Returns.Interfaces;

public interface IReturnsRepository
{
    Task<Return?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Return> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    Task<int> GetReturnedQuantityForOrderItemAsync(Guid orderItemId, CancellationToken cancellationToken = default);
    Task AddAsync(Return returnEntity, CancellationToken cancellationToken = default);
}
