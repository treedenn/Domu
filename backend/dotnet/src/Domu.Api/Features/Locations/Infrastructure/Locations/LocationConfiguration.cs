using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Locations.Infrastructure.Locations;

public sealed class LocationConfiguration : IEntityTypeConfiguration<LocationEntity>
{
    public void Configure(EntityTypeBuilder<LocationEntity> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(location => location.Id);

        builder.Property(location => location.Id)
            .ValueGeneratedNever();

        builder.Property(location => location.OwnerId)
            .IsRequired();

        builder.Property(location => location.Name)
            .HasMaxLength(Domain.Locations.Location.NameMaxLength)
            .IsRequired();

        builder.Property(location => location.Description)
            .HasMaxLength(Domain.Locations.Location.DescriptionMaxLength);

        builder.HasOne<LocationEntity>()
            .WithMany()
            .HasForeignKey(location => location.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(location => new { location.OwnerId, location.ParentId, location.Name });
    }
}
