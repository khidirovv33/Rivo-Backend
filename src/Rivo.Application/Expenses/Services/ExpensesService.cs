using Microsoft.EntityFrameworkCore;
using Rivo.Application.Accounts.Interfaces;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Expenses.Dtos;
using Rivo.Application.Expenses.Interfaces;
using Rivo.Domain.Entities.Expenses;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Expenses.Services;

public class ExpensesService : IExpensesService
{
    private readonly IApplicationDbContext _context;
    private readonly IAccountsService _accounts;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public ExpensesService(IApplicationDbContext context, IAccountsService accounts, ICurrentUserService currentUser, IAuditService audit)
    {
        _context = context;
        _accounts = accounts;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<PaginatedList<ExpenseDto>> GetAllAsync(
        PagedRequest request, DateTime? from, DateTime? to, ExpenseCategory? category, CancellationToken cancellationToken = default)
    {
        var query = _context.Expenses.AsNoTracking().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(x => x.ExpenseDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.ExpenseDate <= to.Value);
        }

        if (category.HasValue)
        {
            query = query.Where(x => x.Category == category.Value);
        }

        var mapped = query.OrderByDescending(x => x.ExpenseDate).Select(x => ToDto(x));
        return await PaginatedList<ExpenseDto>.CreateAsync(mapped, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task<ExpenseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await FindAsync(id, cancellationToken);
        return ToDto(expense);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var expense = new Expense
        {
            AccountId = dto.AccountId,
            Category = dto.Category,
            Amount = dto.Amount,
            ExpenseDate = DateTime.UtcNow,
            Description = dto.Description,
            CreatedBy = _currentUser.UserId,
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        await _accounts.RecordTransactionAsync(
            dto.AccountId, AccountTransactionType.Outflow, dto.Amount,
            $"{dto.Category}: {dto.Description}", "Expense", expense.Id, cancellationToken);

        await _audit.LogAsync("Create", nameof(Expense), expense.Id.ToString(), newValue: dto.Amount.ToString(), cancellationToken: cancellationToken);

        return ToDto(expense);
    }

    public async Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var expense = await FindAsync(id, cancellationToken);
        var oldAmount = expense.Amount;

        // reverse the old ledger effect, then re-apply with the new amount/category
        await _accounts.RecordTransactionAsync(
            expense.AccountId, AccountTransactionType.Inflow, oldAmount,
            "Expense update reversal", "ExpenseReversal", expense.Id, cancellationToken);

        expense.Category = dto.Category;
        expense.Amount = dto.Amount;
        expense.Description = dto.Description;
        expense.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        await _accounts.RecordTransactionAsync(
            expense.AccountId, AccountTransactionType.Outflow, dto.Amount,
            $"{dto.Category}: {dto.Description}", "Expense", expense.Id, cancellationToken);

        await _audit.LogAsync("Update", nameof(Expense), expense.Id.ToString(), oldValue: oldAmount.ToString(), newValue: dto.Amount.ToString(), cancellationToken: cancellationToken);

        return ToDto(expense);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await FindAsync(id, cancellationToken);

        await _accounts.RecordTransactionAsync(
            expense.AccountId, AccountTransactionType.Inflow, expense.Amount,
            "Expense deleted", "ExpenseReversal", expense.Id, cancellationToken);

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", nameof(Expense), expense.Id.ToString(), oldValue: expense.Amount.ToString(), cancellationToken: cancellationToken);
    }

    private async Task<Expense> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Expenses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), id);
    }

    private static ExpenseDto ToDto(Expense expense) => new()
    {
        Id = expense.Id,
        AccountId = expense.AccountId,
        Category = expense.Category,
        Amount = expense.Amount,
        ExpenseDate = expense.ExpenseDate,
        Description = expense.Description,
    };
}
