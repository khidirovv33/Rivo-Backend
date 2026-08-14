using Rivo.Domain.Enums;

namespace Rivo.Application.Products.Dtos;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? MinimumPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int MinimumStock { get; set; }
    public int? MaximumStock { get; set; }
    public decimal TaxRate { get; set; }
    public ProductStatus Status { get; set; }
    public List<ProductVariationDto> Variations { get; set; } = new();
}

public class CreateProductRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
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
}

public class UpdateProductRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
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
    public ProductStatus Status { get; set; }
}
