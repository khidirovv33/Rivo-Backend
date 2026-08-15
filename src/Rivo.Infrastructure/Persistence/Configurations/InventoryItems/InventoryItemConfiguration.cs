using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.InventoryItems;

namespace Rivo.Infrastructure.Persistence.Configurations.InventoryItems;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SystemQuantity).HasPrecision(18, 3);
        builder.Property(x => x.ActualQuantity).HasPrecision(18, 3);
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);

        builder.Ignore(x => x.Difference);
        builder.Ignore(x => x.DifferenceCost);

        builder.HasIndex(x => new { x.InventoryId, x.ProductId, x.ProductVariationId }).IsUnique();
    }
}
