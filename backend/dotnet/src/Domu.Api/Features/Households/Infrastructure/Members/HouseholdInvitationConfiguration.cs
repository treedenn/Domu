using Domu.Api.Features.Households.Domain.Members;
using Domu.Api.Features.Households.Infrastructure.Households;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Households.Infrastructure.Members;

public sealed class HouseholdInvitationConfiguration : IEntityTypeConfiguration<HouseholdInvitationEntity>
{
    public void Configure(EntityTypeBuilder<HouseholdInvitationEntity> builder)
    {
        builder.ToTable("household_invitations");

        builder.HasKey(invitation => invitation.Id);

        builder.Property(invitation => invitation.Id)
            .ValueGeneratedNever();

        builder.Property(invitation => invitation.Email)
            .HasMaxLength(HouseholdInvitation.EmailMaxLength)
            .IsRequired();

        builder.Property(invitation => invitation.DisplayName)
            .HasMaxLength(HouseholdMember.DisplayNameMaxLength)
            .IsRequired();

        builder.Property(invitation => invitation.Token)
            .HasMaxLength(HouseholdInvitation.TokenMaxLength)
            .IsRequired();

        builder.Property(invitation => invitation.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(invitation => invitation.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(invitation => invitation.Token)
            .IsUnique();

        builder.HasIndex(invitation => invitation.InvitedByMemberId);

        builder.HasIndex(invitation => new { invitation.HouseholdId, invitation.Email, invitation.Status });

        builder.HasOne<HouseholdEntity>()
            .WithMany()
            .HasForeignKey(invitation => invitation.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<HouseholdMemberEntity>()
            .WithMany()
            .HasForeignKey(invitation => invitation.InvitedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}