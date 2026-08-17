using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Loyalty;

namespace Rivo.Infrastructure.Persistence.Configurations.Loyalty;

public class LoyaltyLevelConfiguration : IEntityTypeConfiguration<LoyaltyLevel>
{
    public void Configure(EntityTypeBuilder<LoyaltyLevel> builder)
    {
        builder.ToTable("LoyaltyLevels");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MinimumSpend).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountPercentage).HasColumnType("decimal(5,2)");

        builder.HasIndex(x => x.TenantId);
    }
}
