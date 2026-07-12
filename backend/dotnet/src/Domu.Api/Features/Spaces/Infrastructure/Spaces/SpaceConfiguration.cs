using Domu.Api.Features.Households.Infrastructure.Households;
using Domu.Api.Features.Spaces.Domain.Spaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Spaces.Infrastructure.Spaces;

public sealed class SpaceConfiguration : IEntityTypeConfiguration<SpaceEntity>
{
    public void Configure(EntityTypeBuilder<SpaceEntity> builder)
    {
        builder.ToTable("spaces");

        builder.HasKey(space => space.Id);

        builder.Property(space => space.Id)
            .ValueGeneratedNever();

        builder.Property(space => space.HouseholdId)
            .IsRequired();

        builder.Property(space => space.Name)
            .HasMaxLength(Space.NameMaxLength)
            .IsRequired();

        builder.Property(space => space.Description)
            .HasMaxLength(Space.DescriptionMaxLength);

        builder.HasOne<HouseholdEntity>()
            .WithMany()
            .HasForeignKey(space => space.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SpaceEntity>()
            .WithMany()
            .HasForeignKey(space => space.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(space => new { space.HouseholdId, space.ParentId, space.Name });
    }
}