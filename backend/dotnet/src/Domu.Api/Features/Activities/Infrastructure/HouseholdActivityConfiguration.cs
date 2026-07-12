using Domu.Api.Features.Activities.Domain;
using Domu.Api.Features.Households.Infrastructure.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Activities.Infrastructure;

public sealed class HouseholdActivityConfiguration : IEntityTypeConfiguration<HouseholdActivityEntity>
{
    public void Configure(EntityTypeBuilder<HouseholdActivityEntity> builder)
    {
        builder.ToTable("household_events");

        builder.HasKey(householdActivity => householdActivity.Id);

        builder.Property(householdActivity => householdActivity.Id)
            .ValueGeneratedNever();

        builder.Property(householdActivity => householdActivity.OccurredAt)
            .IsRequired();

        builder.Property(householdActivity => householdActivity.ActorId)
            .IsRequired();

        builder.Property(householdActivity => householdActivity.ActorMemberId)
            .IsRequired();

        builder.HasOne<HouseholdMemberEntity>()
            .WithMany()
            .HasForeignKey(householdActivity => householdActivity.ActorMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(householdActivity => householdActivity.Action)
            .HasMaxLength(HouseholdActivity.ActionMaxLength)
            .IsRequired();

        builder.Property(householdActivity => householdActivity.TargetType)
            .HasMaxLength(HouseholdActivity.TargetTypeMaxLength)
            .IsRequired();

        builder.Property(householdActivity => householdActivity.MetadataJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(householdActivity => householdActivity.RequestId)
            .HasMaxLength(128);

        builder.Property(householdActivity => householdActivity.ClientApp)
            .HasMaxLength(64);

        builder.Property(householdActivity => householdActivity.ClientPlatform)
            .HasMaxLength(64);

        builder.Property(householdActivity => householdActivity.ClientVersion)
            .HasMaxLength(64);

        builder.HasIndex(householdActivity => householdActivity.OccurredAt);
        builder.HasIndex(householdActivity => new { householdActivity.HouseholdId, householdActivity.OccurredAt });
        builder.HasIndex(householdActivity => new { householdActivity.ActorId, householdActivity.OccurredAt });
        builder.HasIndex(householdActivity => new { householdActivity.ActorMemberId, householdActivity.OccurredAt });
        builder.HasIndex(householdActivity => new { householdActivity.TargetType, householdActivity.TargetId });
    }
}
