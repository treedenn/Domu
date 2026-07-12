using Domu.Api.Features.Households.Domain.Members;
using Domu.Api.Features.Households.Infrastructure.Households;
using Domu.Api.Features.Users.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Households.Infrastructure.Members;

public sealed class HouseholdMemberConfiguration : IEntityTypeConfiguration<HouseholdMemberEntity>
{
    public void Configure(EntityTypeBuilder<HouseholdMemberEntity> builder)
    {
        builder.ToTable("household_members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .ValueGeneratedNever();

        builder.Property(member => member.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(member => member.UserId)
            .IsRequired();

        builder.Property(member => member.DisplayName)
            .HasMaxLength(HouseholdMember.DisplayNameMaxLength)
            .IsRequired();

        builder.Property(member => member.Archived)
            .IsRequired();

        builder.HasIndex(member => new { member.HouseholdId, member.UserId })
            .IsUnique();

        builder.HasOne<HouseholdEntity>()
            .WithMany()
            .HasForeignKey(member => member.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}