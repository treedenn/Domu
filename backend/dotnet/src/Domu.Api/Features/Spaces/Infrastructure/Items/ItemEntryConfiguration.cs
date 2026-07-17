using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Spaces.Infrastructure.Items;

public sealed class ItemEntryConfiguration : IEntityTypeConfiguration<ItemEntryEntity>
{
    public void Configure(EntityTypeBuilder<ItemEntryEntity> builder)
    {
        builder.ToTable("item_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .ValueGeneratedNever();

        builder.Property(entry => entry.ItemId)
            .IsRequired();

        builder.Property(entry => entry.OriginalQuantity)
            .HasColumnName("original_quantity")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(entry => entry.CurrentQuantity)
            .HasColumnName("current_quantity")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(entry => entry.Unit)
            .HasConversion<int>()
            .IsRequired();

        // Keep writing the legacy column until its removal is applied through a
        // project-owner migration. It is intentionally not part of the domain model.
        builder.Property<int>("LegacyContainerType")
            .HasColumnName("container_type")
            .IsRequired();

        builder.Property(entry => entry.State)
            .HasConversion<int>()
            .IsRequired();
    }
}
