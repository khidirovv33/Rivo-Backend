using Rivo.Domain.Common;
using Rivo.Domain.Entities.Brands;
using Rivo.Domain.Entities.Categories;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Products;

public class Product : BaseEntity, ITenantEntity, ISoftDelete
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? MinimumPrice { get; set; }

    public string Unit { get; set; } = "pcs";
    public int MinimumStock { get; set; }
    public int? MaximumStock { get; set; }
    public decimal TaxRate { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<ProductVariation> Variations { get; set; } = new List<ProductVariation>();
}
