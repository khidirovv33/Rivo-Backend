using Microsoft.EntityFrameworkCore;
using Rivo.Application.Accounts.Dtos;
using Rivo.Application.Accounts.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Domain.Entities.Accounts;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Accounts.Services;

public class AccountsService : IAccountsService
{
    private readonly IApplicationDbContext _context;

    public AccountsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AccountDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Accounts.AsNoTracking().OrderBy(x => x.Name).Select(x => ToDto(x));
        return await PaginatedList<AccountDto>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task<AccountDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await FindAsync(id, cancellationToken);
        return ToDto(account);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = new Account { Name = dto.Name, Type = dto.Type, IsActive = true, Balance = 0 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(account);
    }

    public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = await FindAsync(id, cancellationToken);
        account.Name = dto.Name;
        account.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(account);
    }

    public async Task<PaginatedList<AccountTransactionDto>> GetTransactionsAsync(
        Guid accountId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.AccountTransactions.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AccountTransactionDto
            {
                Id = x.Id,
                AccountId = x.AccountId,
                Type = x.Type,
                Amount = x.Amount,
                BalanceAfter = x.BalanceAfter,
                Description = x.Description,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                CreatedAt = x.CreatedAt,
            });

        return await PaginatedList<AccountTransactionDto>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task<Guid> GetOrCreateDefaultAsync(AccountType type, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Accounts.FirstOrDefaultAsync(x => x.Type == type, cancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        var account = new Account { Name = DefaultNameFor(type), Type = type, IsActive = true, Balance = 0 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account.Id;
    }

    public async Task RecordTransactionAsync(
        Guid accountId, AccountTransactionType type, decimal amount, string? description,
        string? referenceType, Guid? referenceId, CancellationToken cancellationToken = default)
    {
        var account = await FindAsync(accountId, cancellationToken);

        account.Balance += type == AccountTransactionType.Inflow ? amount : -amount;

        var transaction = new AccountTransaction
        {
            AccountId = accountId,
            Type = type,
            Amount = amount,
            BalanceAfter = account.Balance,
            Description = description,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
        };

        _context.AccountTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Account> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), id);
    }

    private static string DefaultNameFor(AccountType type) => type switch
    {
        AccountType.Cash => "Касса",
        AccountType.Bank => "Банковский счёт",
        AccountType.Card => "Эквайринг",
        _ => "Счёт",
    };

    private static AccountDto ToDto(Account account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Type = account.Type,
        Balance = account.Balance,
        IsActive = account.IsActive,
    };
}
