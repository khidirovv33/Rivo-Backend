using Rivo.Application.Accounts.Dtos;
using Rivo.Application.Common.Models;
using Rivo.Domain.Enums;

namespace Rivo.Application.Accounts.Interfaces;

public interface IAccountsService
{
    Task<PaginatedList<AccountDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<AccountDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default);

    Task<AccountDto> UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default);

    Task<PaginatedList<AccountTransactionDto>> GetTransactionsAsync(Guid accountId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>Первый активный счёт нужного типа для tenant'а; создаёт "По умолчанию" при отсутствии.</summary>
    Task<Guid> GetOrCreateDefaultAsync(AccountType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Единая точка движения денег: атомарно меняет Account.Balance и пишет AccountTransaction.
    /// Используется IncomeService/ExpensesService/FinanceIntegrationService — ни один из них не трогает
    /// Account.Balance напрямую.
    /// </summary>
    Task RecordTransactionAsync(
        Guid accountId, AccountTransactionType type, decimal amount, string? description,
        string? referenceType, Guid? referenceId, CancellationToken cancellationToken = default);
}
