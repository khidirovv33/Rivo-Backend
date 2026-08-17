using Rivo.Application.Stores.Dtos;

namespace Rivo.Application.Stores.Interfaces;

public interface IStoresService
{
    Task<StoreDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<List<StoreDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<StoreDto> CreateAsync(Guid tenantId, CreateStoreRequestDto request, CancellationToken cancellationToken = default);
    Task<StoreDto> UpdateAsync(Guid tenantId, Guid id, UpdateStoreRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);

    Task<BranchDto> AddBranchAsync(Guid tenantId, Guid storeId, CreateBranchRequestDto request, CancellationToken cancellationToken = default);
    Task<BranchDto> UpdateBranchAsync(Guid tenantId, Guid storeId, Guid branchId, UpdateBranchRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteBranchAsync(Guid tenantId, Guid storeId, Guid branchId, CancellationToken cancellationToken = default);
}
