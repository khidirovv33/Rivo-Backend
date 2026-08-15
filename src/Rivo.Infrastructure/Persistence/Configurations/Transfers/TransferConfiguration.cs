using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Transfers;

namespace Rivo.Infrastructure.Persistence.Configurations.Transfers;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransferNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => new { x.TenantId, x.TransferNumber }).IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Transfer)
            .HasForeignKey(x => x.TransferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
