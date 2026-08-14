namespace Rivo.Application.Products.Dtos;

public class ProductVariationDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? AttributesJson { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal PriceAdjustment { get; set; }
}

public class CreateProductVariationRequestDto
{
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? AttributesJson { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal PriceAdjustment { get; set; }
}

public class UpdateProductVariationRequestDto
{
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? AttributesJson { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal PriceAdjustment { get; set; }
}
