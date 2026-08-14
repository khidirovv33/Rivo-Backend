using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Products;

namespace Rivo.Infrastructure.Persistence.Configurations.Products;

public class ProductVariationConfiguration : IEntityTypeConfiguration<ProductVariation>
{
    public void Configure(EntityTypeBuilder<ProductVariation> builder)
    {
        builder.ToTable("ProductVariations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Size).HasMaxLength(50);
        builder.Property(x => x.Color).HasMaxLength(50);
        builder.Property(x => x.Barcode).HasMaxLength(100);
        builder.Property(x => x.PriceAdjustment).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Product)
            .WithMany(p => p.Variations)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
