using Domu.Api.Features.Events.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Events.Infrastructure;

public sealed class UserEventConfiguration : IEntityTypeConfiguration<UserEventEntity>
{
    public void Configure(EntityTypeBuilder<UserEventEntity> builder)
    {
        builder.ToTable("user_events");

        builder.HasKey(userEvent => userEvent.Id);

        builder.Property(userEvent => userEvent.Id)
            .ValueGeneratedNever();

        builder.Property(userEvent => userEvent.OccurredAt)
            .IsRequired();

        builder.Property(userEvent => userEvent.ActorUserId)
            .IsRequired();

        builder.Property(userEvent => userEvent.Action)
            .HasMaxLength(UserEvent.ActionMaxLength)
            .IsRequired();

        builder.Property(userEvent => userEvent.TargetType)
            .HasMaxLength(UserEvent.TargetTypeMaxLength)
            .IsRequired();

        builder.Property(userEvent => userEvent.MetadataJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(userEvent => userEvent.RequestId)
            .HasMaxLength(128);

        builder.Property(userEvent => userEvent.ClientApp)
            .HasMaxLength(64);

        builder.Property(userEvent => userEvent.ClientPlatform)
            .HasMaxLength(64);

        builder.Property(userEvent => userEvent.ClientVersion)
            .HasMaxLength(64);

        builder.HasIndex(userEvent => userEvent.OccurredAt);
        builder.HasIndex(userEvent => new { userEvent.HouseholdId, userEvent.OccurredAt });
        builder.HasIndex(userEvent => new { userEvent.ActorUserId, userEvent.OccurredAt });
        builder.HasIndex(userEvent => new { userEvent.TargetType, userEvent.TargetId });
    }
}
