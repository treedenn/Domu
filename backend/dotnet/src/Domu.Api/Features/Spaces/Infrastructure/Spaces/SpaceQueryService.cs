using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Spaces.Infrastructure.Spaces;

public sealed class SpaceQueryService(AppDbContext dbContext) : ISpaceQueryService
{
    public async Task<SpacePage> GetPageAsync(GetSpacesPageQuery query, CancellationToken cancellationToken)
    {
        var baseQuery = dbContext.Spaces
            .AsNoTracking()
            .Where(space => space.HouseholdId == query.HouseholdId);

        baseQuery = query.ParentId is null
            ? baseQuery.Where(space => space.ParentId == null)
            : baseQuery.Where(space => space.ParentId == query.ParentId);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var pageSpaces = await baseQuery
            .OrderBy(space => space.Name)
            .ThenBy(space => space.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(space => new SpaceProjection(
                space.Id,
                space.HouseholdId,
                space.ParentId,
                space.Name,
                space.Description))
            .ToArrayAsync(cancellationToken);

        var spaceIds = pageSpaces.Select(space => space.Id).ToArray();
        var includeItemData = query.Items.HasFlag(SpaceItemsProjection.Data);
        var includeItemCount = query.Items.HasFlag(SpaceItemsProjection.Count) || includeItemData;
        var includeChildSpaceData = query.Children.HasFlag(SpaceChildrenProjection.Data);
        var includeChildSpaceCount = query.Children.HasFlag(SpaceChildrenProjection.Count) || includeChildSpaceData;

        var items = includeItemData
            ? await GetItemsAsync(spaceIds, cancellationToken)
            : new Dictionary<Guid, IReadOnlyList<SpaceItemView>>();
        var itemCounts = includeItemCount && !includeItemData
            ? await GetItemCountsAsync(spaceIds, cancellationToken)
            : new Dictionary<Guid, int>();
        var childSpaces = includeChildSpaceData
            ? await GetChildSpacesAsync(spaceIds, cancellationToken)
            : new Dictionary<Guid, IReadOnlyList<SpaceChildView>>();
        var childSpaceCounts = includeChildSpaceCount && !includeChildSpaceData
            ? await GetChildSpaceCountsAsync(spaceIds, cancellationToken)
            : new Dictionary<Guid, int>();

        return new SpacePage(
            pageSpaces.Select(space => new SpaceView(
                    space.Id,
                    space.HouseholdId,
                    space.ParentId,
                    space.Name,
                    space.Description,
                    BuildCollectionView(includeItemCount, includeItemData, itemCounts, items, space.Id),
                    BuildCollectionView(
                        includeChildSpaceCount,
                        includeChildSpaceData,
                        childSpaceCounts,
                        childSpaces,
                        space.Id)))
                .ToArray(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }

    private static CollectionView<T>? BuildCollectionView<T>(
        bool includeCount,
        bool includeData,
        IReadOnlyDictionary<Guid, int> counts,
        IReadOnlyDictionary<Guid, IReadOnlyList<T>> data,
        Guid ownerId)
    {
        if (!includeCount && !includeData)
            return null;

        var collectionData = includeData ? data.GetValueOrDefault(ownerId, []) : null;
        var count = includeData
            ? collectionData!.Count
            : counts.GetValueOrDefault(ownerId);

        return new CollectionView<T>(count, collectionData);
    }

    private async Task<Dictionary<Guid, int>> GetItemCountsAsync(
        IReadOnlyCollection<Guid> spaceIds,
        CancellationToken cancellationToken)
    {
        if (spaceIds.Count == 0)
            return [];

        return await dbContext.Items
            .AsNoTracking()
            .Where(item => spaceIds.Contains(item.SpaceId))
            .GroupBy(item => item.SpaceId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Count, cancellationToken);
    }

    private async Task<Dictionary<Guid, IReadOnlyList<SpaceItemView>>> GetItemsAsync(
        IReadOnlyCollection<Guid> spaceIds,
        CancellationToken cancellationToken)
    {
        if (spaceIds.Count == 0)
            return [];

        var itemTotals = await dbContext.ItemEntries
            .AsNoTracking()
            .Where(entry => dbContext.Items
                .Where(item => spaceIds.Contains(item.SpaceId))
                .Select(item => item.Id)
                .Contains(entry.ItemId))
            .GroupBy(entry => entry.ItemId)
            .Select(group => new { ItemId = group.Key, TotalQuantity = group.Sum(entry => entry.Quantity) })
            .ToDictionaryAsync(entry => entry.ItemId, entry => entry.TotalQuantity, cancellationToken);

        var items = await dbContext.Items
            .AsNoTracking()
            .Where(item => spaceIds.Contains(item.SpaceId))
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Select(item => new
            {
                item.SpaceId,
                View = new SpaceItemView(
                    item.Id,
                    item.SpaceId,
                    item.Name,
                    item.Category,
                    item.Barcode,
                    0)
            })
            .ToArrayAsync(cancellationToken);

        return items
            .GroupBy(item => item.SpaceId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<SpaceItemView>)group
                    .Select(item => item.View with { TotalQuantity = itemTotals.GetValueOrDefault(item.View.Id) })
                    .ToArray());
    }

    private async Task<Dictionary<Guid, IReadOnlyList<SpaceChildView>>> GetChildSpacesAsync(
        IReadOnlyCollection<Guid> spaceIds,
        CancellationToken cancellationToken)
    {
        if (spaceIds.Count == 0)
            return [];

        var children = await dbContext.Spaces
            .AsNoTracking()
            .Where(space => space.ParentId.HasValue && spaceIds.Contains(space.ParentId.Value))
            .OrderBy(space => space.Name)
            .ThenBy(space => space.Id)
            .Select(space => new
            {
                ParentId = space.ParentId!.Value,
                View = new SpaceChildView(
                    space.Id,
                    space.HouseholdId,
                    space.ParentId,
                    space.Name,
                    space.Description)
            })
            .ToArrayAsync(cancellationToken);

        return children
            .GroupBy(child => child.ParentId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<SpaceChildView>)group.Select(child => child.View).ToArray());
    }

    private async Task<Dictionary<Guid, int>> GetChildSpaceCountsAsync(
        IReadOnlyCollection<Guid> spaceIds,
        CancellationToken cancellationToken)
    {
        if (spaceIds.Count == 0)
            return [];

        return await dbContext.Spaces
            .AsNoTracking()
            .Where(space => space.ParentId.HasValue && spaceIds.Contains(space.ParentId.Value))
            .GroupBy(space => space.ParentId!.Value)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Count, cancellationToken);
    }

    private sealed record SpaceProjection(
        Guid Id,
        Guid HouseholdId,
        Guid? ParentId,
        string Name,
        string? Description);
}
