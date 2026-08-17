using AutoMapper;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Stores.Dtos;
using Rivo.Application.Stores.Interfaces;
using Rivo.Domain.Entities.Stores;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Stores.Services;

public class StoresService : IStoresService
{
    private readonly IStoresRepository _storesRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public StoresService(IStoresRepository storesRepository, IApplicationDbContext dbContext, IMapper mapper)
    {
        _storesRepository = storesRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<StoreDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var store = await GetTenantStoreOrThrowAsync(tenantId, id, cancellationToken);
        return _mapper.Map<StoreDto>(store);
    }

    public async Task<List<StoreDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var stores = await _storesRepository.GetByTenantAsync(tenantId, cancellationToken);
        return stores.Select(s => _mapper.Map<StoreDto>(s)).ToList();
    }

    public async Task<StoreDto> CreateAsync(Guid tenantId, CreateStoreRequestDto request, CancellationToken cancellationToken = default)
    {
        var store = new Store
        {
            TenantId = tenantId,
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            Currency = request.Currency,
            DefaultTaxRate = request.DefaultTaxRate,
            OpeningHours = request.OpeningHours
        };

        await _storesRepository.AddAsync(store, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StoreDto>(store);
    }

    public async Task<StoreDto> UpdateAsync(Guid tenantId, Guid id, UpdateStoreRequestDto request, CancellationToken cancellationToken = default)
    {
        var store = await GetTenantStoreOrThrowAsync(tenantId, id, cancellationToken);

        store.Name = request.Name;
        store.LogoUrl = request.LogoUrl;
        store.Address = request.Address;
        store.Phone = request.Phone;
        store.Email = request.Email;
        store.Status = request.Status;
        store.Currency = request.Currency;
        store.DefaultTaxRate = request.DefaultTaxRate;
        store.OpeningHours = request.OpeningHours;
        store.UpdatedAt = DateTime.UtcNow;

        _storesRepository.Update(store);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StoreDto>(store);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var store = await GetTenantStoreOrThrowAsync(tenantId, id, cancellationToken);
        store.IsDeleted = true;
        store.DeletedAt = DateTime.UtcNow;
        _storesRepository.Update(store);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BranchDto> AddBranchAsync(Guid tenantId, Guid storeId, CreateBranchRequestDto request, CancellationToken cancellationToken = default)
    {
        await GetTenantStoreOrThrowAsync(tenantId, storeId, cancellationToken);

        var branch = new Branch
        {
            TenantId = tenantId,
            StoreId = storeId,
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone
        };

        await _storesRepository.AddBranchAsync(branch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<BranchDto> UpdateBranchAsync(Guid tenantId, Guid storeId, Guid branchId, UpdateBranchRequestDto request, CancellationToken cancellationToken = default)
    {
        await GetTenantStoreOrThrowAsync(tenantId, storeId, cancellationToken);
        var branch = await GetTenantBranchOrThrowAsync(tenantId, storeId, branchId, cancellationToken);

        branch.Name = request.Name;
        branch.Address = request.Address;
        branch.Phone = request.Phone;
        branch.Status = request.Status;
        branch.UpdatedAt = DateTime.UtcNow;

        _storesRepository.UpdateBranch(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BranchDto>(branch);
    }

    public async Task DeleteBranchAsync(Guid tenantId, Guid storeId, Guid branchId, CancellationToken cancellationToken = default)
    {
        await GetTenantStoreOrThrowAsync(tenantId, storeId, cancellationToken);
        var branch = await GetTenantBranchOrThrowAsync(tenantId, storeId, branchId, cancellationToken);

        branch.IsDeleted = true;
        branch.DeletedAt = DateTime.UtcNow;
        _storesRepository.UpdateBranch(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Store> GetTenantStoreOrThrowAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var store = await _storesRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Store), id);

        if (store.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return store;
    }

    private async Task<Branch> GetTenantBranchOrThrowAsync(Guid tenantId, Guid storeId, Guid branchId, CancellationToken cancellationToken)
    {
        var branch = await _storesRepository.GetBranchByIdAsync(branchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), branchId);

        if (branch.TenantId != tenantId || branch.StoreId != storeId)
        {
            throw new TenantMismatchException();
        }

        return branch;
    }
}
