using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Loyalty;

namespace Rivo.Infrastructure.Persistence.Configurations.Loyalty;

public class LoyaltyCardConfiguration : IEntityTypeConfiguration<LoyaltyCard>
{
    public void Configure(EntityTypeBuilder<LoyaltyCard> builder)
    {
        builder.ToTable("LoyaltyCards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CardNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(x => x.CardNumber).IsUnique();
        builder.HasIndex(x => x.CustomerId).IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LoyaltyLevel)
            .WithMany()
            .HasForeignKey(x => x.LoyaltyLevelId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
