using Rivo.Domain.Enums;

namespace Rivo.Application.Accounts.Dtos;

public class AccountTransactionDto
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public AccountTransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Description { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; }
}
