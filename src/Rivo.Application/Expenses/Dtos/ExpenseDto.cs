using Rivo.Domain.Enums;

namespace Rivo.Application.Expenses.Dtos;

public class ExpenseDto
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public ExpenseCategory Category { get; set; }

    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; }

    public string? Description { get; set; }
}

public class CreateExpenseDto
{
    public Guid AccountId { get; set; }

    public ExpenseCategory Category { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }
}

public class UpdateExpenseDto
{
    public ExpenseCategory Category { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }
}
