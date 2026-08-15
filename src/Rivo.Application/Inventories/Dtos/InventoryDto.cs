using Rivo.Application.InventoryItems.Dtos;
using Rivo.Domain.Enums;

namespace Rivo.Application.Inventories.Dtos;

public class InventoryDto
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }

    public string InventoryNumber { get; set; } = null!;

    public InventoryStatus Status { get; set; }

    public Guid ResponsibleUserId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? Notes { get; set; }

    public List<InventoryItemDto> Items { get; set; } = [];

    public decimal ShortageQuantity => Items.Where(i => i.Difference < 0).Sum(i => -i.Difference);

    public decimal SurplusQuantity => Items.Where(i => i.Difference > 0).Sum(i => i.Difference);

    public decimal ShortageCost => Items.Where(i => i.Difference < 0).Sum(i => -i.DifferenceCost);

    public decimal SurplusCost => Items.Where(i => i.Difference > 0).Sum(i => i.DifferenceCost);
}

public class CreateInventoryDto
{
    public Guid WarehouseId { get; set; }

    public string? Notes { get; set; }
}
