using Rivo.Application.Common.Models;
using Rivo.Application.Expenses.Dtos;
using Rivo.Domain.Enums;

namespace Rivo.Application.Expenses.Interfaces;

public interface IExpensesService
{
    Task<PaginatedList<ExpenseDto>> GetAllAsync(
        PagedRequest request, DateTime? from, DateTime? to, ExpenseCategory? category, CancellationToken cancellationToken = default);

    Task<ExpenseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, CancellationToken cancellationToken = default);

    Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
