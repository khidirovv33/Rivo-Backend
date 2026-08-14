using Rivo.Domain.Entities.Products;

namespace Rivo.Application.Products.Interfaces;

public interface IProductsRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(Guid tenantId, string barcode, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(Guid tenantId, string sku, CancellationToken cancellationToken = default);
    Task<(List<Product> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    void Remove(Product product);

    Task<ProductVariation?> GetVariationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddVariationAsync(ProductVariation variation, CancellationToken cancellationToken = default);
    void UpdateVariation(ProductVariation variation);
    void RemoveVariation(ProductVariation variation);
}
