using Domu.Api.Features.Locations.Infrastructure.Items;
using Domu.Api.Features.Locations.Infrastructure.Locations;
using Domu.Api.Features.Users.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ItemEntity> Items => Set<ItemEntity>();
    public DbSet<ItemEntryEntity> ItemEntries => Set<ItemEntryEntity>();
    public DbSet<LocationEntity> Locations => Set<LocationEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
