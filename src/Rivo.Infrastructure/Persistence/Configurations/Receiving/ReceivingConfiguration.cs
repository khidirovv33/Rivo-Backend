using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReceivingEntity = Rivo.Domain.Entities.Receiving.Receiving;

namespace Rivo.Infrastructure.Persistence.Configurations.Receiving;

public class ReceivingConfiguration : IEntityTypeConfiguration<ReceivingEntity>
{
    public void Configure(EntityTypeBuilder<ReceivingEntity> builder)
    {
        builder.ToTable("Receivings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Receiving)
            .HasForeignKey(x => x.ReceivingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
