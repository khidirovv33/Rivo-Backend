using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockEntity = Rivo.Domain.Entities.Stock.Stock;

namespace Rivo.Infrastructure.Persistence.Configurations.Stock;

public class StockConfiguration : IEntityTypeConfiguration<StockEntity>
{
    public void Configure(EntityTypeBuilder<StockEntity> builder)
    {
        builder.ToTable("Stocks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SystemQuantity).HasPrecision(18, 3);
        builder.Property(x => x.ReservedQuantity).HasPrecision(18, 3);

        builder.Ignore(x => x.AvailableQuantity);

        builder.HasIndex(x => new { x.WarehouseId, x.ProductId, x.ProductVariationId }).IsUnique();

        // Зеркалим soft-delete фильтр родителя (Warehouse), иначе EF предупреждает о рассинхроне
        // required-навигации с global query filter.
        builder.HasQueryFilter(x => !x.Warehouse.IsDeleted);
    }
}
