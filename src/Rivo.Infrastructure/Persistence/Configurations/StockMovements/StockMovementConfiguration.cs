using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.StockMovements;

namespace Rivo.Infrastructure.Persistence.Configurations.StockMovements;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.QuantityBefore).HasPrecision(18, 3);
        builder.Property(x => x.QuantityAfter).HasPrecision(18, 3);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ReferenceType).HasMaxLength(100);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.WarehouseId, x.ProductId, x.CreatedAt });
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });

        // Зеркалим soft-delete фильтр родителя (Warehouse) — см. StockConfiguration.
        builder.HasQueryFilter(x => !x.Warehouse.IsDeleted);
    }
}
