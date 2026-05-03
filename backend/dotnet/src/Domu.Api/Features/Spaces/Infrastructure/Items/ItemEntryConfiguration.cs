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

        builder.Property(entry => entry.Quantity)
            .IsRequired();

        builder.Property(entry => entry.State)
            .HasConversion<int>()
            .IsRequired();
    }
}
