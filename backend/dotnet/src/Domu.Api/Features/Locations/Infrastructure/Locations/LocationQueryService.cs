using Domu.Api.Features.Locations.Application.Locations;
using Domu.Api.Features.Locations.Application.Locations.Contracts;
using Domu.Api.Features.Locations.Application.Locations.Ports;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Locations.Infrastructure.Locations;

public sealed class LocationQueryService(AppDbContext dbContext) : ILocationQueryService
{
    public async Task<LocationPage> GetPageAsync(GetLocationsPageQuery query, CancellationToken cancellationToken)
    {
        var baseQuery = dbContext.Locations
            .AsNoTracking()
            .Where(location => location.OwnerId == query.OwnerId);

        baseQuery = query.ParentId is null
            ? baseQuery.Where(location => location.ParentId == null)
            : baseQuery.Where(location => location.ParentId == query.ParentId);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var pageLocations = await baseQuery
            .OrderBy(location => location.Name)
            .ThenBy(location => location.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(location => new LocationProjection(
                location.Id,
                location.OwnerId,
                location.ParentId,
                location.Name,
                location.Description))
            .ToArrayAsync(cancellationToken);

        var locationIds = pageLocations.Select(location => location.Id).ToArray();
        var itemCounts = query.Items.HasFlag(LocationItemsProjection.Count)
            ? await GetItemCountsAsync(locationIds, cancellationToken)
            : new Dictionary<Guid, int>();
        var items = query.Items.HasFlag(LocationItemsProjection.Data)
            ? await GetItemsAsync(locationIds, cancellationToken)
            : new Dictionary<Guid, IReadOnlyList<LocationItemView>>();
        var childLocationCounts = query.Children.HasFlag(LocationChildrenProjection.Count)
            ? await GetChildLocationCountsAsync(locationIds, cancellationToken)
            : new Dictionary<Guid, int>();
        var childLocations = query.Children.HasFlag(LocationChildrenProjection.Data)
            ? await GetChildLocationsAsync(locationIds, cancellationToken)
            : new Dictionary<Guid, IReadOnlyList<LocationChildView>>();

        return new LocationPage(
            pageLocations.Select(location => new LocationView(
                    location.Id,
                    location.OwnerId,
                    location.ParentId,
                    location.Name,
                    location.Description,
                    query.Items.HasFlag(LocationItemsProjection.Count) ? itemCounts.GetValueOrDefault(location.Id) : null,
                    query.Items.HasFlag(LocationItemsProjection.Data) ? items.GetValueOrDefault(location.Id, []) : null,
                    query.Children.HasFlag(LocationChildrenProjection.Count) ? childLocationCounts.GetValueOrDefault(location.Id) : null,
                    query.Children.HasFlag(LocationChildrenProjection.Data) ? childLocations.GetValueOrDefault(location.Id, []) : null))
                .ToArray(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }

    private async Task<Dictionary<Guid, int>> GetItemCountsAsync(
        IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0)
            return [];

        return await dbContext.Items
            .AsNoTracking()
            .Where(item => locationIds.Contains(item.LocationId))
            .GroupBy(item => item.LocationId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Count, cancellationToken);
    }

    private async Task<Dictionary<Guid, IReadOnlyList<LocationItemView>>> GetItemsAsync(
        IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0)
            return [];

        var itemTotals = await dbContext.ItemEntries
            .AsNoTracking()
            .Where(entry => dbContext.Items
                .Where(item => locationIds.Contains(item.LocationId))
                .Select(item => item.Id)
                .Contains(entry.ItemId))
            .GroupBy(entry => entry.ItemId)
            .Select(group => new { ItemId = group.Key, TotalQuantity = group.Sum(entry => entry.Quantity) })
            .ToDictionaryAsync(entry => entry.ItemId, entry => entry.TotalQuantity, cancellationToken);

        var items = await dbContext.Items
            .AsNoTracking()
            .Where(item => locationIds.Contains(item.LocationId))
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Select(item => new
            {
                item.LocationId,
                View = new LocationItemView(
                    item.Id,
                    item.LocationId,
                    item.Name,
                    item.Category,
                    item.Barcode,
                    0)
            })
            .ToArrayAsync(cancellationToken);

        return items
            .GroupBy(item => item.LocationId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<LocationItemView>)group
                    .Select(item => item.View with { TotalQuantity = itemTotals.GetValueOrDefault(item.View.Id) })
                    .ToArray());
    }

    private async Task<Dictionary<Guid, IReadOnlyList<LocationChildView>>> GetChildLocationsAsync(
        IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0)
            return [];

        var children = await dbContext.Locations
            .AsNoTracking()
            .Where(location => location.ParentId.HasValue && locationIds.Contains(location.ParentId.Value))
            .OrderBy(location => location.Name)
            .ThenBy(location => location.Id)
            .Select(location => new
            {
                ParentId = location.ParentId!.Value,
                View = new LocationChildView(
                    location.Id,
                    location.OwnerId,
                    location.ParentId,
                    location.Name,
                    location.Description)
            })
            .ToArrayAsync(cancellationToken);

        return children
            .GroupBy(child => child.ParentId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<LocationChildView>)group.Select(child => child.View).ToArray());
    }

    private async Task<Dictionary<Guid, int>> GetChildLocationCountsAsync(
        IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0)
            return [];

        return await dbContext.Locations
            .AsNoTracking()
            .Where(location => location.ParentId.HasValue && locationIds.Contains(location.ParentId.Value))
            .GroupBy(location => location.ParentId!.Value)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Count, cancellationToken);
    }

    private sealed record LocationProjection(
        Guid Id,
        Guid OwnerId,
        Guid? ParentId,
        string Name,
        string? Description);
}
