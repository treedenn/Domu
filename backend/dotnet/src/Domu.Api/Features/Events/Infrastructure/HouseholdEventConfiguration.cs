using Domu.Api.Features.Events.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domu.Api.Features.Households.Infrastructure.Members;

namespace Domu.Api.Features.Events.Infrastructure;

public sealed class HouseholdEventConfiguration : IEntityTypeConfiguration<HouseholdEventEntity>
{
    public void Configure(EntityTypeBuilder<HouseholdEventEntity> builder)
    {
        builder.ToTable("household_events");

        builder.HasKey(userEvent => userEvent.Id);

        builder.Property(userEvent => userEvent.Id)
            .ValueGeneratedNever();

        builder.Property(userEvent => userEvent.OccurredAt)
            .IsRequired();

        builder.Property(userEvent => userEvent.ActorMemberId)
            .IsRequired();

        builder.HasOne<HouseholdMemberEntity>()
            .WithMany()
            .HasForeignKey(userEvent => userEvent.ActorMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(userEvent => userEvent.Action)
            .HasMaxLength(HouseholdEvent.ActionMaxLength)
            .IsRequired();

        builder.Property(userEvent => userEvent.TargetType)
            .HasMaxLength(HouseholdEvent.TargetTypeMaxLength)
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
        builder.HasIndex(userEvent => new { userEvent.ActorMemberId, userEvent.OccurredAt });
        builder.HasIndex(userEvent => new { userEvent.TargetType, userEvent.TargetId });
    }
}
