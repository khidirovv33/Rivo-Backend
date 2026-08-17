using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Inventories;

namespace Rivo.Infrastructure.Persistence.Configurations.Inventories;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InventoryNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => new { x.TenantId, x.InventoryNumber }).IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Inventory)
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
