using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Accounts;

namespace Rivo.Infrastructure.Persistence.Configurations.Accounts;

public class AccountTransactionConfiguration : IEntityTypeConfiguration<AccountTransaction>
{
    public void Configure(EntityTypeBuilder<AccountTransaction> builder)
    {
        builder.ToTable("AccountTransactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ReferenceType).HasMaxLength(100);

        builder.HasIndex(x => new { x.AccountId, x.CreatedAt });
    }
}
