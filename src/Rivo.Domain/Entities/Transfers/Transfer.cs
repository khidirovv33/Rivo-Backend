using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Transfers;

/// <summary>Перемещение товара между складами/филиалами: Store #1 -> Transfer -> Store #2 (раздел 10 ТЗ).</summary>
public class Transfer : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public Guid SourceWarehouseId { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public string TransferNumber { get; set; } = null!;

    public TransferStatus Status { get; set; } = TransferStatus.Draft;

    public DateTime TransferDate { get; set; }

    public string? Notes { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public ICollection<TransferItem> Items { get; set; } = new List<TransferItem>();
}
