using AwesomeAssertions;
using Rivo.Application.Accounts.Dtos;
using Rivo.Application.Accounts.Services;
using Rivo.Application.Audit.Services;
using Rivo.Application.Expenses.Dtos;
using Rivo.Application.Expenses.Services;
using Rivo.Domain.Enums;
using Rivo.IntegrationTests.Common;
using Xunit;

namespace Rivo.IntegrationTests.FinanceIntelligence;

/// <summary>
/// DoD: "ключевые финансовые действия пишутся в Audit Log" + Account.Balance must always match its
/// AccountTransaction history, including through Expense update/delete (reverse-then-reapply, not a
/// direct row edit).
/// </summary>
public class ExpensesAffectAccountBalanceTests
{
    [Fact]
    public async Task Creating_an_expense_decreases_the_account_balance()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var accounts = new AccountsService(context);
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var notifications = new FakeNotificationsService();
        var expenses = new ExpensesService(context, accounts, notifications, currentUser, audit);

        var account = await accounts.CreateAsync(new CreateAccountDto { Name = "Касса", Type = AccountType.Cash });

        await expenses.CreateAsync(new CreateExpenseDto
        {
            AccountId = account.Id,
            Category = ExpenseCategory.Rent,
            Amount = 400m,
            Description = "Аренда",
        });

        var updated = await accounts.GetByIdAsync(account.Id);
        updated.Balance.Should().Be(-400m);
    }

    [Fact]
    public async Task Updating_an_expense_amount_reverses_the_old_ledger_effect_before_reapplying_the_new_one()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var accounts = new AccountsService(context);
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var expenses = new ExpensesService(context, accounts, new FakeNotificationsService(), currentUser, audit);

        var account = await accounts.CreateAsync(new CreateAccountDto { Name = "Касса", Type = AccountType.Cash });
        var expense = await expenses.CreateAsync(new CreateExpenseDto
        {
            AccountId = account.Id,
            Category = ExpenseCategory.Transport,
            Amount = 100m,
        });

        await expenses.UpdateAsync(expense.Id, new UpdateExpenseDto
        {
            Category = ExpenseCategory.Transport,
            Amount = 250m,
        });

        var updated = await accounts.GetByIdAsync(account.Id);
        updated.Balance.Should().Be(-250m);
    }

    [Fact]
    public async Task Deleting_an_expense_restores_the_account_balance()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var accounts = new AccountsService(context);
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var expenses = new ExpensesService(context, accounts, new FakeNotificationsService(), currentUser, audit);

        var account = await accounts.CreateAsync(new CreateAccountDto { Name = "Касса", Type = AccountType.Cash });
        var expense = await expenses.CreateAsync(new CreateExpenseDto
        {
            AccountId = account.Id,
            Category = ExpenseCategory.Other,
            Amount = 150m,
        });

        await expenses.DeleteAsync(expense.Id);

        var updated = await accounts.GetByIdAsync(account.Id);
        updated.Balance.Should().Be(0m);
    }
}
