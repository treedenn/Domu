using Domu.Api.Features.Households.Infrastructure.Households;
using Domu.Api.Features.Spaces.Infrastructure.Items;
using Domu.Api.Features.Spaces.Infrastructure.Spaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.ShoppingLists.Infrastructure.Items;

public sealed class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItemEntity>
{
    public void Configure(EntityTypeBuilder<ShoppingListItemEntity> builder)
    {
        builder.ToTable("shopping_list_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.Name)
            .HasMaxLength(Domain.Items.ShoppingListItem.NameMaxLength)
            .IsRequired();

        builder.Property(item => item.NormalizedName)
            .HasMaxLength(Domain.Items.ShoppingListItem.NameMaxLength)
            .IsRequired();

        builder.Property(item => item.ContainerUnit)
            .HasMaxLength(Domain.Items.ShoppingListItem.UnitMaxLength);

        builder.Property(item => item.Note)
            .HasMaxLength(Domain.Items.ShoppingListItem.NoteMaxLength);

        builder.HasOne<HouseholdEntity>()
            .WithMany()
            .HasForeignKey(item => item.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SpaceEntity>()
            .WithMany()
            .HasForeignKey(item => item.SpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ItemEntity>()
            .WithMany()
            .HasForeignKey(item => item.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.HouseholdId);
        builder.HasIndex(item => item.ShoppingListId);
        builder.HasIndex(item => new { item.ShoppingListId, item.Checked });
        builder.HasIndex(item => new { item.ShoppingListId, item.SortOrder });
        builder.HasIndex(item => item.SpaceId);
        builder.HasIndex(item => item.ItemId);
    }
}
