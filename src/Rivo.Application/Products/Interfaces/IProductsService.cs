using Rivo.Application.Common.Models;
using Rivo.Application.Products.Dtos;

namespace Rivo.Application.Products.Interfaces;

public interface IProductsService
{
    Task<ProductDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto?> FindByBarcodeAsync(Guid tenantId, string barcode, CancellationToken cancellationToken = default);
    Task<PaginatedList<ProductDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(Guid tenantId, CreateProductRequestDto request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid tenantId, Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);

    Task<ProductVariationDto> AddVariationAsync(Guid tenantId, Guid productId, CreateProductVariationRequestDto request, CancellationToken cancellationToken = default);
    Task<ProductVariationDto> UpdateVariationAsync(Guid tenantId, Guid productId, Guid variationId, UpdateProductVariationRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteVariationAsync(Guid tenantId, Guid productId, Guid variationId, CancellationToken cancellationToken = default);
}
