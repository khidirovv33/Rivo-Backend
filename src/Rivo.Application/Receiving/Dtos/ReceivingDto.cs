using Rivo.Domain.Enums;

namespace Rivo.Application.Receiving.Dtos;

public class ReceivingDto
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid WarehouseId { get; set; }

    public DateTime ReceivingDate { get; set; }

    public ReceivingStatus Status { get; set; }

    public string? Notes { get; set; }

    public List<ReceivingItemDto> Items { get; set; } = [];
}

public class CreateReceivingDto
{
    public Guid PurchaseOrderId { get; set; }

    public string? Notes { get; set; }

    public List<CreateReceivingItemDto> Items { get; set; } = [];
}
