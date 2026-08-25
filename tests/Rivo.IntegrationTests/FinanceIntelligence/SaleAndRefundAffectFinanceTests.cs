using AwesomeAssertions;
using Rivo.Application.Accounts.Services;
using Rivo.Application.Audit.Services;
using Rivo.Application.Income.Services;
using Rivo.Domain.Enums;
using Rivo.IntegrationTests.Common;
using Xunit;

namespace Rivo.IntegrationTests.FinanceIntelligence;

/// <summary>DoD: "продажа и возврат корректно отражаются в финансовых показателях" (§12 ТЗ).</summary>
public class SaleAndRefundAffectFinanceTests
{
    [Fact]
    public async Task Recording_a_sale_creates_positive_income_and_increases_the_cash_account_balance()
    {
        await using var context = TestDbContextFactory.Create();
        var accounts = new AccountsService(context);
        var income = new IncomeService(context, accounts, new AuditService(context, new FakeCurrentUserService(), new FakeCurrentTenantService(), new FakeDateTimeService()));
        var financeIntegration = new FinanceIntegrationService(income);

        var orderId = Guid.NewGuid();
        await financeIntegration.RecordSaleAsync(Guid.NewGuid(), orderId, 1500m);

        var incomeEntry = context.Incomes.Single(x => x.ReferenceId == orderId);
        incomeEntry.Type.Should().Be(IncomeType.Sale);
        incomeEntry.Amount.Should().Be(1500m);

        var cashAccountId = await accounts.GetOrCreateDefaultAsync(AccountType.Cash);
        var cashAccount = await accounts.GetByIdAsync(cashAccountId);
        cashAccount.Balance.Should().Be(1500m);
    }

    [Fact]
    public async Task Recording_a_refund_creates_negative_income_and_decreases_the_cash_account_balance()
    {
        await using var context = TestDbContextFactory.Create();
        var accounts = new AccountsService(context);
        var income = new IncomeService(context, accounts, new AuditService(context, new FakeCurrentUserService(), new FakeCurrentTenantService(), new FakeDateTimeService()));
        var financeIntegration = new FinanceIntegrationService(income);

        await financeIntegration.RecordSaleAsync(Guid.NewGuid(), Guid.NewGuid(), 1000m);

        var returnId = Guid.NewGuid();
        await financeIntegration.RecordRefundAsync(Guid.NewGuid(), returnId, 300m);

        var refundEntry = context.Incomes.Single(x => x.ReferenceId == returnId);
        refundEntry.Type.Should().Be(IncomeType.Refund);
        refundEntry.Amount.Should().Be(-300m);

        var cashAccountId = await accounts.GetOrCreateDefaultAsync(AccountType.Cash);
        var cashAccount = await accounts.GetByIdAsync(cashAccountId);
        cashAccount.Balance.Should().Be(700m);
    }
}
