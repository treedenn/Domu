using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Search;
using Domu.Api.Features.Spaces.Application.Search.Contracts;
using Domu.Api.Features.Spaces.Application.Search.Ports;
using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Spaces.Infrastructure.Search;

public sealed class SpacesAndItemsSearchService(AppDbContext dbContext) : ISpacesAndItemsSearchService
{
    public async Task<SearchResultsView> SearchAsync(
        SearchSpacesAndItemsQuery query,
        CancellationToken cancellationToken)
    {
        var text = query.Text?.Trim();
        var normalizedText = text?.ToLowerInvariant();
        var hasText = !string.IsNullOrWhiteSpace(normalizedText);
        var hasExpiryFilter = query.ExpiringWithinDays is not null;

        if (!hasText && !hasExpiryFilter)
            return new SearchResultsView([], []);

        var spaces = hasText
            ? await SearchSpacesAsync(query.HouseholdId, normalizedText!, query.Limit, cancellationToken)
            : [];

        var items = await SearchItemsAsync(
            query.HouseholdId,
            normalizedText,
            query.ExpiringWithinDays,
            query.Limit,
            cancellationToken);

        return new SearchResultsView(spaces, items);
    }

    private async Task<IReadOnlyList<SpaceSearchResultView>> SearchSpacesAsync(
        Guid householdId,
        string normalizedText,
        int limit,
        CancellationToken cancellationToken)
    {
        return await dbContext.Spaces
            .AsNoTracking()
            .Where(space => space.HouseholdId == householdId
                            && (space.Name.ToLower().Contains(normalizedText)
                                || (space.Description != null
                                    && space.Description.ToLower().Contains(normalizedText))))
            .OrderBy(space => space.Name.ToLower().StartsWith(normalizedText) ? 0 : 1)
            .ThenBy(space => space.Name)
            .ThenBy(space => space.Id)
            .Take(limit)
            .Select(space => new SpaceSearchResultView(
                space.Id,
                space.HouseholdId,
                space.ParentId,
                space.Name,
                space.Description))
            .ToArrayAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ItemSearchResultView>> SearchItemsAsync(
        Guid householdId,
        string? normalizedText,
        int? expiringWithinDays,
        int limit,
        CancellationToken cancellationToken)
    {
        var hasText = !string.IsNullOrWhiteSpace(normalizedText);
        var now = DateTimeOffset.UtcNow;
        var expiresBefore = expiringWithinDays is null
            ? (DateTimeOffset?)null
            : now.AddDays(expiringWithinDays.Value);

        var baseQuery =
            from item in dbContext.Items.AsNoTracking()
            join space in dbContext.Spaces.AsNoTracking() on item.SpaceId equals space.Id
            where space.HouseholdId == householdId
            select item;

        if (hasText)
            baseQuery = baseQuery.Where(item =>
                item.Name.ToLower().Contains(normalizedText!)
                || (item.Category != null && item.Category.ToLower().Contains(normalizedText!))
                || (item.Barcode != null && item.Barcode.ToLower() == normalizedText));

        if (expiresBefore is not null)
            baseQuery = baseQuery.Where(item => dbContext.ItemEntries
                .Any(entry => entry.ItemId == item.Id
                              && entry.ExpirationDate != null
                              && entry.ExpirationDate >= now
                              && entry.ExpirationDate <= expiresBefore.Value));

        var itemRows = await baseQuery
            .OrderBy(item => hasText && item.Barcode != null && item.Barcode.ToLower() == normalizedText ? 0 : 1)
            .ThenBy(item => hasText && item.Name.ToLower().StartsWith(normalizedText!) ? 0 : 1)
            .ThenBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Take(limit)
            .Select(item => new
            {
                item.Id,
                item.SpaceId,
                item.Name,
                item.Category,
                item.Barcode
            })
            .ToArrayAsync(cancellationToken);

        var itemIds = itemRows.Select(item => item.Id).ToArray();
        var entriesByItemId = await GetEntriesByItemIdAsync(itemIds, cancellationToken);

        return itemRows
            .Select(item =>
            {
                var entries = entriesByItemId.GetValueOrDefault(item.Id, []);
                return new ItemSearchResultView(
                    item.Id,
                    item.SpaceId,
                    item.Name,
                    item.Category,
                    item.Barcode,
                    entries.Sum(entry => entry.CurrentQuantity),
                    entries);
            })
            .ToArray();
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<ItemEntryView>>> GetEntriesByItemIdAsync(
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken)
    {
        if (itemIds.Count == 0)
            return new Dictionary<Guid, IReadOnlyList<ItemEntryView>>();

        var entries = await dbContext.ItemEntries
            .AsNoTracking()
            .Where(entry => itemIds.Contains(entry.ItemId))
            .OrderBy(entry => entry.ExpirationDate)
            .ThenBy(entry => entry.Id)
            .Select(entry => new
            {
                entry.ItemId,
                View = new ItemEntryView(
                    entry.Id,
                    entry.InitialQuantity,
                    entry.CurrentQuantity,
                    entry.Unit,
                    entry.ContainerType,
                    entry.State,
                    entry.AcquisitionDate,
                    entry.ExpirationDate)
            })
            .ToArrayAsync(cancellationToken);

        return entries
            .GroupBy(entry => entry.ItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ItemEntryView>)group.Select(entry => entry.View).ToArray());
    }
}