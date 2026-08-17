using AutoMapper;
using Rivo.Application.Categories.Dtos;
using Rivo.Application.Categories.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Domain.Entities.Categories;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Categories.Services;

public class CategoriesService : ICategoriesService
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public CategoriesService(ICategoriesRepository categoriesRepository, IApplicationDbContext dbContext, IMapper mapper)
    {
        _categoriesRepository = categoriesRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<CategoryDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var category = await GetTenantCategoryOrThrowAsync(tenantId, id, cancellationToken);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<List<CategoryDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var categories = await _categoriesRepository.GetByTenantAsync(tenantId, cancellationToken);
        return categories.Select(c => _mapper.Map<CategoryDto>(c)).ToList();
    }

    public async Task<CategoryDto> CreateAsync(Guid tenantId, CreateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };

        await _categoriesRepository.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid tenantId, Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var category = await GetTenantCategoryOrThrowAsync(tenantId, id, cancellationToken);

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;
        category.UpdatedAt = DateTime.UtcNow;

        _categoriesRepository.Update(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var category = await GetTenantCategoryOrThrowAsync(tenantId, id, cancellationToken);
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        _categoriesRepository.Update(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Category> GetTenantCategoryOrThrowAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoriesRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        if (category.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return category;
    }
}
