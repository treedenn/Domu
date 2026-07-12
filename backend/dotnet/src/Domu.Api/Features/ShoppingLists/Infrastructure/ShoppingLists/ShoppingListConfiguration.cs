using Domu.Api.Features.Households.Infrastructure.Households;
using Domu.Api.Features.Households.Infrastructure.Members;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.ShoppingLists.Infrastructure.ShoppingLists;

public sealed class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingListEntity>
{
    public void Configure(EntityTypeBuilder<ShoppingListEntity> builder)
    {
        builder.ToTable("shopping_lists");

        builder.HasKey(shoppingList => shoppingList.Id);

        builder.Property(shoppingList => shoppingList.Id)
            .ValueGeneratedNever();

        builder.Property(shoppingList => shoppingList.Name)
            .HasMaxLength(ShoppingList.NameMaxLength)
            .IsRequired();

        builder.Property(shoppingList => shoppingList.HouseholdId)
            .IsRequired();

        builder.Property(shoppingList => shoppingList.CreatedByMemberId)
            .IsRequired();

        builder.Property(shoppingList => shoppingList.CreatedAt)
            .IsRequired();

        builder.Property(shoppingList => shoppingList.UpdatedAt)
            .IsRequired();

        builder.HasMany(shoppingList => shoppingList.Items)
            .WithOne()
            .HasForeignKey(item => item.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<HouseholdEntity>()
            .WithMany()
            .HasForeignKey(shoppingList => shoppingList.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<HouseholdMemberEntity>()
            .WithMany()
            .HasForeignKey(shoppingList => shoppingList.CreatedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(shoppingList => shoppingList.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(shoppingList => shoppingList.HouseholdId);
        builder.HasIndex(shoppingList => shoppingList.CreatedByMemberId);

        builder.HasIndex(shoppingList => new { shoppingList.HouseholdId, shoppingList.IsDefault });

        builder.HasIndex(shoppingList => new { shoppingList.HouseholdId, shoppingList.IsDefault })
            .IsUnique()
            .HasDatabaseName("ix_shopping_lists_household_id_active_default")
            .HasFilter("is_default = true AND archived_at IS NULL");
    }
}