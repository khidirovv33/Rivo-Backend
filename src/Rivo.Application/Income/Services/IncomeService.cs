using Microsoft.EntityFrameworkCore;
using Rivo.Application.Accounts.Interfaces;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Income.Dtos;
using Rivo.Application.Income.Interfaces;
using Rivo.Domain.Enums;
using IncomeEntity = Rivo.Domain.Entities.Income.Income;

namespace Rivo.Application.Income.Services;

public class IncomeService : IIncomeService
{
    private readonly IApplicationDbContext _context;
    private readonly IAccountsService _accounts;
    private readonly IAuditService _audit;

    public IncomeService(IApplicationDbContext context, IAccountsService accounts, IAuditService audit)
    {
        _context = context;
        _accounts = accounts;
        _audit = audit;
    }

    public async Task<PaginatedList<IncomeDto>> GetAllAsync(
        PagedRequest request, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.Incomes.AsNoTracking().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(x => x.IncomeDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.IncomeDate <= to.Value);
        }

        var mapped = query.OrderByDescending(x => x.IncomeDate).Select(x => ToDto(x));
        return await PaginatedList<IncomeDto>.CreateAsync(mapped, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task<IncomeDto> CreateAsync(CreateIncomeDto dto, CancellationToken cancellationToken = default)
    {
        return await RecordAsync(IncomeType.Other, dto.Amount, dto.Description, "Manual", null, cancellationToken, dto.AccountId);
    }

    public async Task<IncomeDto> RecordAsync(
        IncomeType type, decimal amount, string? description, string? referenceType, Guid? referenceId,
        CancellationToken cancellationToken = default) =>
        await RecordAsync(type, amount, description, referenceType, referenceId, cancellationToken, null);

    private async Task<IncomeDto> RecordAsync(
        IncomeType type, decimal amount, string? description, string? referenceType, Guid? referenceId,
        CancellationToken cancellationToken, Guid? accountId)
    {
        var resolvedAccountId = accountId ?? await _accounts.GetOrCreateDefaultAsync(AccountType.Cash, cancellationToken);
        var signedAmount = type == IncomeType.Refund ? -amount : amount;

        var income = new IncomeEntity
        {
            AccountId = resolvedAccountId,
            Type = type,
            Amount = signedAmount,
            IncomeDate = DateTime.UtcNow,
            Description = description,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
        };

        _context.Incomes.Add(income);
        await _context.SaveChangesAsync(cancellationToken);

        await _accounts.RecordTransactionAsync(
            resolvedAccountId,
            type == IncomeType.Refund ? AccountTransactionType.Outflow : AccountTransactionType.Inflow,
            amount,
            description ?? type.ToString(),
            "Income",
            income.Id,
            cancellationToken);

        await _audit.LogAsync($"Income:{type}", nameof(IncomeEntity), income.Id.ToString(), newValue: signedAmount.ToString(), cancellationToken: cancellationToken);

        return ToDto(income);
    }

    private static IncomeDto ToDto(IncomeEntity income) => new()
    {
        Id = income.Id,
        AccountId = income.AccountId,
        Type = income.Type,
        Amount = income.Amount,
        IncomeDate = income.IncomeDate,
        Description = income.Description,
        ReferenceType = income.ReferenceType,
        ReferenceId = income.ReferenceId,
    };
}
