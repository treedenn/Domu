using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Spaces.Infrastructure.Items;

public sealed class ItemConfiguration : IEntityTypeConfiguration<ItemEntity>
{
    public void Configure(EntityTypeBuilder<ItemEntity> builder)
    {
        builder.ToTable("items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.SpaceId)
            .IsRequired();

        builder.Property(item => item.Name)
            .HasMaxLength(Domain.Items.Item.NameMaxLength)
            .IsRequired();

        builder.Property(item => item.Category)
            .HasMaxLength(Domain.Items.Item.CategoryMaxLength);

        builder.Property(item => item.Barcode)
            .HasMaxLength(Domain.Items.Item.BarcodeMaxLength);

        builder.HasMany(item => item.Entries)
            .WithOne()
            .HasForeignKey(entry => entry.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(item => item.Entries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(item => new { item.SpaceId, item.Name });
    }
}
