using AutoMapper;
using Rivo.Application.Brands.Dtos;
using Rivo.Application.Brands.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Domain.Entities.Brands;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Brands.Services;

public class BrandsService : IBrandsService
{
    private readonly IBrandsRepository _brandsRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public BrandsService(IBrandsRepository brandsRepository, IApplicationDbContext dbContext, IMapper mapper)
    {
        _brandsRepository = brandsRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<BrandDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var brand = await GetTenantBrandOrThrowAsync(tenantId, id, cancellationToken);
        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<List<BrandDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var brands = await _brandsRepository.GetByTenantAsync(tenantId, cancellationToken);
        return brands.Select(b => _mapper.Map<BrandDto>(b)).ToList();
    }

    public async Task<BrandDto> CreateAsync(Guid tenantId, CreateBrandRequestDto request, CancellationToken cancellationToken = default)
    {
        var brand = new Brand
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            LogoUrl = request.LogoUrl
        };

        await _brandsRepository.AddAsync(brand, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<BrandDto> UpdateAsync(Guid tenantId, Guid id, UpdateBrandRequestDto request, CancellationToken cancellationToken = default)
    {
        var brand = await GetTenantBrandOrThrowAsync(tenantId, id, cancellationToken);

        brand.Name = request.Name;
        brand.Description = request.Description;
        brand.LogoUrl = request.LogoUrl;
        brand.UpdatedAt = DateTime.UtcNow;

        _brandsRepository.Update(brand);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var brand = await GetTenantBrandOrThrowAsync(tenantId, id, cancellationToken);
        brand.IsDeleted = true;
        brand.DeletedAt = DateTime.UtcNow;
        _brandsRepository.Update(brand);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Brand> GetTenantBrandOrThrowAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var brand = await _brandsRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Brand), id);

        if (brand.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return brand;
    }
}
