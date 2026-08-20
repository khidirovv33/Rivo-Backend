using Rivo.Domain.Enums;

namespace Rivo.Application.Income.Dtos;

public class IncomeDto
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public IncomeType Type { get; set; }

    public decimal Amount { get; set; }

    public DateTime IncomeDate { get; set; }

    public string? Description { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }
}

/// <summary>Ручное поступление ("другие поступления" — раздел 12 ТЗ), не продажа/возврат.</summary>
public class CreateIncomeDto
{
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }
}
