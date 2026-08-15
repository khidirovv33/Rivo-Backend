using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Transfers;

public class TransferItem : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid TransferId { get; set; }

    public Transfer Transfer { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Guid? ProductVariationId { get; set; }

    public decimal Quantity { get; set; }
}
