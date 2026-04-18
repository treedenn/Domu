using Domu.Api.Features.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Users.Infrastructure;

public sealed class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.ToTable("actors");

        builder.HasKey(actor => actor.Id);

        builder.Property(actor => actor.Id)
            .ValueGeneratedNever();

        builder.Property(actor => actor.ExternalIdentifier)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(actor => actor.ExternalIdentifier)
            .IsUnique();

        builder.Property(actor => actor.SubscriptionTier)
            .IsRequired();
    }
}
