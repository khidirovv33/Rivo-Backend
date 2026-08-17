using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Returns;

namespace Rivo.Infrastructure.Persistence.Configurations.Returns;

public class ReturnConfiguration : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.ToTable("Returns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.TotalRefundAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.TenantId);

        builder.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ProcessedByUser).WithMany().HasForeignKey(x => x.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
