using Rivo.Domain.Enums;

namespace Rivo.Application.PurchaseOrders.Dtos;

public class PurchaseOrderDto
{
    public Guid Id { get; set; }

    public Guid SupplierId { get; set; }

    public Guid WarehouseId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public PurchaseOrderStatus Status { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public string? Notes { get; set; }

    public decimal TotalAmount { get; set; }

    public List<PurchaseOrderItemDto> Items { get; set; } = [];
}

public class CreatePurchaseOrderDto
{
    public Guid SupplierId { get; set; }

    public Guid WarehouseId { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public string? Notes { get; set; }

    public List<CreatePurchaseOrderItemDto> Items { get; set; } = [];
}
