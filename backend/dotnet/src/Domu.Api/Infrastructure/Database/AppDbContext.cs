using Domu.Api.Features.Households.Infrastructure.Households;
using Domu.Api.Features.Households.Infrastructure.Members;
using Domu.Api.Features.Events.Infrastructure;
using Domu.Api.Features.ShoppingLists.Infrastructure.Items;
using Domu.Api.Features.ShoppingLists.Infrastructure.ShoppingLists;
using Domu.Api.Features.Spaces.Infrastructure.Items;
using Domu.Api.Features.Spaces.Infrastructure.Spaces;
using Domu.Api.Features.Users.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<HouseholdEntity> Households => Set<HouseholdEntity>();
    public DbSet<HouseholdInvitationEntity> HouseholdInvitations => Set<HouseholdInvitationEntity>();
    public DbSet<HouseholdMemberEntity> HouseholdMembers => Set<HouseholdMemberEntity>();
    public DbSet<ItemEntity> Items => Set<ItemEntity>();
    public DbSet<ItemEntryEntity> ItemEntries => Set<ItemEntryEntity>();
    public DbSet<ShoppingListEntity> ShoppingLists => Set<ShoppingListEntity>();
    public DbSet<ShoppingListItemEntity> ShoppingListItems => Set<ShoppingListItemEntity>();
    public DbSet<SpaceEntity> Spaces => Set<SpaceEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<HouseholdEventEntity> HouseholdEvents => Set<HouseholdEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
