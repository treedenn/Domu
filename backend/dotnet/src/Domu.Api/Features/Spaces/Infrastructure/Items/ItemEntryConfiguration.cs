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

        builder.Property(entry => entry.Count).HasColumnName("count").IsRequired();
        builder.Property(entry => entry.OriginalAmountPerUnit).HasColumnName("original_amount_per_unit").HasPrecision(18, 3);
        builder.Property(entry => entry.CurrentAmountPerUnit).HasColumnName("current_amount_per_unit").HasPrecision(18, 3);

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

        builder.HasIndex(entry => new { entry.ItemId, entry.ExpirationDate });
    }
}
