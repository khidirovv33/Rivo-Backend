using Microsoft.EntityFrameworkCore;
using Rivo.Application.Products.Interfaces;
using Rivo.Domain.Entities.Products;

namespace Rivo.Infrastructure.Persistence.Repositories.Products;

public class ProductsRepository : IProductsRepository
{
    private readonly ApplicationDbContext _context;

    public ProductsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private IQueryable<Product> ProductsWithIncludes() =>
        _context.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Variations);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        ProductsWithIncludes().IgnoreQueryFilters().Where(p => !p.IsDeleted).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetByBarcodeAsync(Guid tenantId, string barcode, CancellationToken cancellationToken = default) =>
        ProductsWithIncludes().FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == barcode, cancellationToken);

    public Task<Product?> GetBySkuAsync(Guid tenantId, string sku, CancellationToken cancellationToken = default) =>
        _context.Products.IgnoreQueryFilters()
            .Where(p => !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Sku == sku, cancellationToken);

    public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = ProductsWithIncludes().Where(p => p.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.Sku.Contains(searchTerm) || (p.Barcode != null && p.Barcode.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await _context.Products.AddAsync(product, cancellationToken);

    public void Update(Product product) => _context.Products.Update(product);

    public void Remove(Product product) => _context.Products.Remove(product);

    public Task<ProductVariation?> GetVariationByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ProductVariations.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task AddVariationAsync(ProductVariation variation, CancellationToken cancellationToken = default) =>
        await _context.ProductVariations.AddAsync(variation, cancellationToken);

    public void UpdateVariation(ProductVariation variation) => _context.ProductVariations.Update(variation);

    public void RemoveVariation(ProductVariation variation) => _context.ProductVariations.Remove(variation);
}
