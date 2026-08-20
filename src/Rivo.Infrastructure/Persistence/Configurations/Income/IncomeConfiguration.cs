using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IncomeEntity = Rivo.Domain.Entities.Income.Income;

namespace Rivo.Infrastructure.Persistence.Configurations.Income;

public class IncomeConfiguration : IEntityTypeConfiguration<IncomeEntity>
{
    public void Configure(EntityTypeBuilder<IncomeEntity> builder)
    {
        builder.ToTable("Incomes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ReferenceType).HasMaxLength(100);

        builder.HasIndex(x => x.IncomeDate);
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
    }
}
