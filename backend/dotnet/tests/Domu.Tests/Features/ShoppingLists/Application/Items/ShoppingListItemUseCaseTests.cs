using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.Items;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Tests.Features.ShoppingLists.Application.Items;

public sealed class ShoppingListItemUseCaseTests
{
    [Fact]
    public async Task CreateItem_ValidatesSpaceBelongsToHousehold()
    {
        var repository = new FakeShoppingListRepository();
        var householdId = Guid.NewGuid();
        var shoppingList = repository.AddDefaultList(householdId);
        var useCase = new CreateShoppingListItemUseCase(repository, repository, new FakeHouseholdAccessService());

        var action = () => useCase.ExecuteAsync(
            new CreateShoppingListItemCommand(
                Guid.NewGuid(),
                householdId,
                shoppingList.Id,
                "Milk",
                null,
                null,
                null,
                null,
                Guid.NewGuid(),
                null),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Empty(repository.Items);
    }

    [Fact]
    public async Task CreateItem_PersistsCleanedItem()
    {
        var repository = new FakeShoppingListRepository();
        var householdId = Guid.NewGuid();
        var spaceId = repository.AddHouseholdSpace(householdId);
        var itemId = repository.AddHouseholdItem(householdId);
        var shoppingList = repository.AddDefaultList(householdId);
        var useCase = new CreateShoppingListItemUseCase(repository, repository, new FakeHouseholdAccessService());

        var result = await useCase.ExecuteAsync(
            new CreateShoppingListItemCommand(
                Guid.NewGuid(),
                householdId,
                shoppingList.Id,
                "  Milk   chocolate ",
                2,
                1,
                " l ",
                "  chilled  ",
                spaceId,
                itemId),
            CancellationToken.None);

        Assert.Equal("Milk chocolate", result.Name);
        Assert.Equal("milk chocolate", result.NormalizedName);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(1, result.ContainerQuantity);
        Assert.Equal("l", result.ContainerUnit);
        Assert.Equal("chilled", result.Note);
        Assert.Equal(spaceId, result.SpaceId);
        Assert.Equal(itemId, result.ItemId);
        Assert.Equal(1, repository.AddItemCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task CheckItem_RequiresItemInRouteList()
    {
        var repository = new FakeShoppingListRepository();
        var householdId = Guid.NewGuid();
        var routeList = repository.AddDefaultList(householdId);
        var otherList = repository.AddDefaultList(Guid.NewGuid());
        var item = repository.AddItem(householdId, otherList.Id, "Milk");
        var useCase = new CheckShoppingListItemUseCase(repository, repository, new FakeHouseholdAccessService());

        var action = () => useCase.ExecuteAsync(
            new ShoppingListItemCommand(Guid.NewGuid(), householdId, routeList.Id, item.Id),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class FakeHouseholdAccessService : IHouseholdAccessService
    {
        public Task EnsureCanAccessHouseholdAsync(
            Guid householdId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeShoppingListRepository : IShoppingListRepository, IShoppingListItemRepository
    {
        private readonly HashSet<(Guid SpaceId, Guid HouseholdId)> _spaces = [];
        private readonly HashSet<(Guid ItemId, Guid HouseholdId)> _items = [];

        public List<ShoppingList> Lists { get; } = [];
        public List<ShoppingListItem> Items { get; } = [];
        public int AddItemCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public ShoppingList AddDefaultList(Guid householdId)
        {
            var now = DateTimeOffset.UtcNow;
            var shoppingList = new ShoppingList(
                Guid.NewGuid(),
                householdId,
                "Shopping list",
                true,
                Guid.NewGuid(),
                now,
                now);
            Lists.Add(shoppingList);
            return shoppingList;
        }

        public ShoppingListItem AddItem(Guid householdId, Guid shoppingListId, string name)
        {
            var now = DateTimeOffset.UtcNow;
            var item = new ShoppingListItem(
                Guid.NewGuid(),
                householdId,
                shoppingListId,
                name,
                Guid.NewGuid(),
                now,
                now,
                Items.Count + 1);
            Items.Add(item);
            return item;
        }

        public Guid AddHouseholdSpace(Guid householdId)
        {
            var spaceId = Guid.NewGuid();
            _spaces.Add((spaceId, householdId));
            return spaceId;
        }

        public Guid AddHouseholdItem(Guid householdId)
        {
            var itemId = Guid.NewGuid();
            _items.Add((itemId, householdId));
            return itemId;
        }

        public Task<ShoppingList?> GetActiveDefaultByHouseholdAsync(
            Guid householdId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Lists.SingleOrDefault(list =>
                list.HouseholdId == householdId && list.IsDefault && list.ArchivedAt is null));
        }

        public Task<ShoppingList?> GetByIdAsync(Guid shoppingListId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Lists.SingleOrDefault(list => list.Id == shoppingListId));
        }

        public Task AddAsync(ShoppingList shoppingList, CancellationToken cancellationToken)
        {
            Lists.Add(shoppingList);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ShoppingListItem>> GetItemsAsync(
            Guid shoppingListId,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<ShoppingListItem> items = Items
                .Where(item => item.ShoppingListId == shoppingListId)
                .ToArray();
            return Task.FromResult(items);
        }

        public Task<ShoppingListItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(item => item.Id == itemId));
        }

        public Task<decimal> GetNextSortOrderAsync(Guid shoppingListId, CancellationToken cancellationToken)
        {
            return Task.FromResult((decimal)(Items.Count(item => item.ShoppingListId == shoppingListId) + 1));
        }

        public Task<bool> SpaceBelongsToHouseholdAsync(
            Guid spaceId,
            Guid householdId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_spaces.Contains((spaceId, householdId)));
        }

        public Task<bool> ItemBelongsToHouseholdAsync(
            Guid itemId,
            Guid householdId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_items.Contains((itemId, householdId)));
        }

        public Task AddAsync(ShoppingListItem item, CancellationToken cancellationToken)
        {
            AddItemCalls++;
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ShoppingListItem item, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid itemId, CancellationToken cancellationToken)
        {
            Items.RemoveAll(item => item.Id == itemId);
            return Task.CompletedTask;
        }

        public Task<int> DeleteCheckedAsync(Guid shoppingListId, CancellationToken cancellationToken)
        {
            var removed = Items.RemoveAll(item => item.ShoppingListId == shoppingListId && item.Checked);
            return Task.FromResult(removed);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
