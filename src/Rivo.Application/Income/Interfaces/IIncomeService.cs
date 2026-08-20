using Rivo.Application.Common.Models;
using Rivo.Application.Income.Dtos;
using Rivo.Domain.Enums;

namespace Rivo.Application.Income.Interfaces;

public interface IIncomeService
{
    Task<PaginatedList<IncomeDto>> GetAllAsync(
        PagedRequest request, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);

    /// <summary>Ручное "прочее поступление".</summary>
    Task<IncomeDto> CreateAsync(CreateIncomeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Системная запись (продажа/возврат) — используется FinanceIntegrationService, а не напрямую с API.
    /// Amount всегда положительный; знак движения денег определяется typeом (Sale = приход, Refund = расход).
    /// </summary>
    Task<IncomeDto> RecordAsync(
        IncomeType type, decimal amount, string? description, string? referenceType, Guid? referenceId,
        CancellationToken cancellationToken = default);
}
