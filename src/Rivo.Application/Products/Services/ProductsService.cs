using AutoMapper;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Products.Dtos;
using Rivo.Application.Products.Interfaces;
using Rivo.Domain.Entities.Products;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Products.Services;

public class ProductsService : IProductsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProductsService(IProductsRepository productsRepository, IApplicationDbContext dbContext, IMapper mapper)
    {
        _productsRepository = productsRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ProductDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetTenantProductOrThrowAsync(tenantId, id, cancellationToken);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> FindByBarcodeAsync(Guid tenantId, string barcode, CancellationToken cancellationToken = default)
    {
        var product = await _productsRepository.GetByBarcodeAsync(tenantId, barcode, cancellationToken);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<PaginatedList<ProductDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _productsRepository.GetPagedAsync(
            tenantId, request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken);

        var dtos = items.Select(p => _mapper.Map<ProductDto>(p)).ToList();
        return new PaginatedList<ProductDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<ProductDto> CreateAsync(Guid tenantId, CreateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _productsRepository.GetBySkuAsync(tenantId, request.Sku, cancellationToken) is not null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.Sku)] = new[] { "A product with this SKU already exists." }
            });
        }

        var product = new Product
        {
            TenantId = tenantId,
            Name = request.Name,
            Sku = request.Sku,
            Barcode = request.Barcode,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            WholesalePrice = request.WholesalePrice,
            MinimumPrice = request.MinimumPrice,
            Unit = request.Unit,
            MinimumStock = request.MinimumStock,
            MaximumStock = request.MaximumStock,
            TaxRate = request.TaxRate
        };

        await _productsRepository.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid tenantId, Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        var product = await GetTenantProductOrThrowAsync(tenantId, id, cancellationToken);

        product.Name = request.Name;
        product.Sku = request.Sku;
        product.Barcode = request.Barcode;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.Description = request.Description;
        product.ImageUrl = request.ImageUrl;
        product.PurchasePrice = request.PurchasePrice;
        product.SellingPrice = request.SellingPrice;
        product.WholesalePrice = request.WholesalePrice;
        product.MinimumPrice = request.MinimumPrice;
        product.Unit = request.Unit;
        product.MinimumStock = request.MinimumStock;
        product.MaximumStock = request.MaximumStock;
        product.TaxRate = request.TaxRate;
        product.Status = request.Status;
        product.UpdatedAt = DateTime.UtcNow;

        _productsRepository.Update(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetTenantProductOrThrowAsync(tenantId, id, cancellationToken);
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        _productsRepository.Update(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductVariationDto> AddVariationAsync(Guid tenantId, Guid productId, CreateProductVariationRequestDto request, CancellationToken cancellationToken = default)
    {
        await GetTenantProductOrThrowAsync(tenantId, productId, cancellationToken);

        var variation = new ProductVariation
        {
            ProductId = productId,
            Size = request.Size,
            Color = request.Color,
            AttributesJson = request.AttributesJson,
            Sku = request.Sku,
            Barcode = request.Barcode,
            PriceAdjustment = request.PriceAdjustment
        };

        await _productsRepository.AddVariationAsync(variation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductVariationDto>(variation);
    }

    public async Task<ProductVariationDto> UpdateVariationAsync(Guid tenantId, Guid productId, Guid variationId, UpdateProductVariationRequestDto request, CancellationToken cancellationToken = default)
    {
        await GetTenantProductOrThrowAsync(tenantId, productId, cancellationToken);
        var variation = await GetProductVariationOrThrowAsync(productId, variationId, cancellationToken);

        variation.Size = request.Size;
        variation.Color = request.Color;
        variation.AttributesJson = request.AttributesJson;
        variation.Sku = request.Sku;
        variation.Barcode = request.Barcode;
        variation.PriceAdjustment = request.PriceAdjustment;
        variation.UpdatedAt = DateTime.UtcNow;

        _productsRepository.UpdateVariation(variation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductVariationDto>(variation);
    }

    public async Task DeleteVariationAsync(Guid tenantId, Guid productId, Guid variationId, CancellationToken cancellationToken = default)
    {
        await GetTenantProductOrThrowAsync(tenantId, productId, cancellationToken);
        var variation = await GetProductVariationOrThrowAsync(productId, variationId, cancellationToken);

        _productsRepository.RemoveVariation(variation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Product> GetTenantProductOrThrowAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var product = await _productsRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        if (product.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return product;
    }

    private async Task<ProductVariation> GetProductVariationOrThrowAsync(Guid productId, Guid variationId, CancellationToken cancellationToken)
    {
        var variation = await _productsRepository.GetVariationByIdAsync(variationId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductVariation), variationId);

        if (variation.ProductId != productId)
        {
            throw new NotFoundException(nameof(ProductVariation), variationId);
        }

        return variation;
    }
}
