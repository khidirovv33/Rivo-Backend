using Rivo.Domain.Entities.Stores;

namespace Rivo.Application.Stores.Interfaces;

public interface IStoresRepository
{
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Store>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Store store, CancellationToken cancellationToken = default);
    void Update(Store store);
    void Remove(Store store);

    Task<Branch?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddBranchAsync(Branch branch, CancellationToken cancellationToken = default);
    void UpdateBranch(Branch branch);
    void RemoveBranch(Branch branch);
}
