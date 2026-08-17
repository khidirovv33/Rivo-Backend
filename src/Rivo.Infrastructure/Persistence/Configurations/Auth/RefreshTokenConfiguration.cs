using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rivo.Domain.Entities.Auth;

namespace Rivo.Infrastructure.Persistence.Configurations.Auth;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Token).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Token).IsUnique();
        builder.Property(x => x.CreatedByIp).HasMaxLength(64);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.IsExpired);
        builder.Ignore(x => x.IsRevoked);
        builder.Ignore(x => x.IsActive);
    }
}
