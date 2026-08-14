using Rivo.Domain.Enums;

namespace Rivo.Application.Returns.Dtos;

public class ReturnDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProcessedByUserId { get; set; }
    public string? Reason { get; set; }
    public decimal TotalRefundAmount { get; set; }
    public ReturnStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReturnItemDto> Items { get; set; } = new();
}
