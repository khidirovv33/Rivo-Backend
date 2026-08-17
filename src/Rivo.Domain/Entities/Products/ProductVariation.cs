using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Products;

/// <summary>A sellable variant of a Product distinguished by attributes such as size/color.</summary>
public class ProductVariation : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? AttributesJson { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal PriceAdjustment { get; set; }
}
