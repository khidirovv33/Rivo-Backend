namespace Rivo.Application.Stock.Dtos;

public class StockDto
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public Guid? ProductVariationId { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal ReservedQuantity { get; set; }

    public decimal AvailableQuantity { get; set; }
}

public class ReserveStockDto
{
    public Guid WarehouseId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal Quantity { get; set; }

    public string? Reason { get; set; }
}
