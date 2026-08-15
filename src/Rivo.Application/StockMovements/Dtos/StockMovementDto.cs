using Rivo.Domain.Enums;

namespace Rivo.Application.StockMovements.Dtos;

public class StockMovementDto
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public StockMovementType Type { get; set; }

    public decimal Quantity { get; set; }

    public decimal QuantityBefore { get; set; }

    public decimal QuantityAfter { get; set; }

    public string? Reason { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateStockMovementDto
{
    public Guid WarehouseId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public StockMovementType Type { get; set; }

    /// <summary>Знаковая дельта: положительная — приход/возврат/трансфер-ин, отрицательная — расход/продажа/списание/трансфер-аут.</summary>
    public decimal Quantity { get; set; }

    public string? Reason { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }
}
