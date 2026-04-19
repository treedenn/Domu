using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Users.Infrastructure;

public sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.ExternalIdentifier)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(user => user.ExternalIdentifier)
            .IsUnique();

        builder.Property(user => user.SubscriptionTier)
            .IsRequired();
    }
}
