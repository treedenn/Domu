using Domu.Api.Features.Households.Infrastructure.Households;
using Domu.Api.Features.Households.Infrastructure.Members;
using Domu.Api.Features.ShoppingLists.Domain.Items;
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
            .HasMaxLength(ShoppingListItem.NameMaxLength)
            .IsRequired();

        builder.Property(item => item.NormalizedName)
            .HasMaxLength(ShoppingListItem.NameMaxLength)
            .IsRequired();

        builder.Property(item => item.Note)
            .HasMaxLength(ShoppingListItem.NoteMaxLength);

        builder.Property(item => item.AmountPerUnit).HasPrecision(18, 3);
        builder.Property(item => item.Count).IsRequired();
        builder.Property(item => item.Unit).HasConversion<int>();

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
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<HouseholdMemberEntity>()
            .WithMany()
            .HasForeignKey(item => item.AddedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<HouseholdMemberEntity>()
            .WithMany()
            .HasForeignKey(item => item.CheckedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.HouseholdId);
        builder.HasIndex(item => item.ShoppingListId);
        builder.HasIndex(item => new { item.ShoppingListId, item.Checked });
        builder.HasIndex(item => new { item.ShoppingListId, item.SortOrder });
        builder.HasIndex(item => item.AddedByMemberId);
        builder.HasIndex(item => item.CheckedByMemberId);
        builder.HasIndex(item => item.SpaceId);
        builder.HasIndex(item => item.ItemId);
    }
}
