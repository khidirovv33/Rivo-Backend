using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Receiving;

namespace Rivo.Infrastructure.Persistence.Configurations.Receiving;

public class ReceivingItemConfiguration : IEntityTypeConfiguration<ReceivingItem>
{
    public void Configure(EntityTypeBuilder<ReceivingItem> builder)
    {
        builder.ToTable("ReceivingItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuantityReceived).HasPrecision(18, 3);
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);
    }
}
