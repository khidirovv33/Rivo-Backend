using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Stores;

namespace Rivo.Infrastructure.Persistence.Configurations.Stores;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(x => x.TenantId);

        builder.HasOne(x => x.Store)
            .WithMany(s => s.Branches)
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
