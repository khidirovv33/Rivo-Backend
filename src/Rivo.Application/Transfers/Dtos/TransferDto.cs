using Rivo.Domain.Enums;

namespace Rivo.Application.Transfers.Dtos;

public class TransferDto
{
    public Guid Id { get; set; }

    public Guid SourceWarehouseId { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public string TransferNumber { get; set; } = null!;

    public TransferStatus Status { get; set; }

    public DateTime TransferDate { get; set; }

    public string? Notes { get; set; }

    public List<TransferItemDto> Items { get; set; } = [];
}

public class CreateTransferDto
{
    public Guid SourceWarehouseId { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public string? Notes { get; set; }

    public List<CreateTransferItemDto> Items { get; set; } = [];
}
