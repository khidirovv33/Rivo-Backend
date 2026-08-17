using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Permissions;

namespace Rivo.Infrastructure.Persistence.Configurations.Permissions;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Module).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(300);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
