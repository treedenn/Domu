using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Tests.Features.ShoppingLists.Application.ShoppingLists;

public sealed class ShoppingListUseCaseTests
{
    [Fact]
    public async Task Create_CreatesNamedListForHousehold()
    {
        var repository = new FakeShoppingListRepository();
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var useCase = new CreateShoppingListUseCase(repository, new FakeHouseholdAccessService(memberId));

        var result = await useCase.ExecuteAsync(
            new CreateShoppingListCommand(userId, householdId, "  Weekly   groceries "), CancellationToken.None);

        Assert.Equal("Weekly groceries", result.Name);
        Assert.Equal(householdId, result.HouseholdId);
        Assert.Equal(memberId, result.CreatedByMemberId);
        Assert.Single(repository.Lists);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task GetLists_ReturnsOnlyActiveHouseholdLists()
    {
        var repository = new FakeShoppingListRepository();
        var householdId = Guid.NewGuid();
        repository.AddList(householdId, "Weekly");
        repository.AddList(Guid.NewGuid(), "Other household");
        var archived = repository.AddList(householdId, "Archived");
        archived.Archive(DateTimeOffset.UtcNow);
        var useCase = new GetShoppingListsUseCase(repository, new FakeHouseholdAccessService());

        var result = await useCase.ExecuteAsync(
            new GetShoppingListsQuery(Guid.NewGuid(), householdId), CancellationToken.None);

        Assert.Collection(result, list => Assert.Equal("Weekly", list.Name));
    }

    [Fact]
    public async Task Update_RenamesAccessibleList()
    {
        var repository = new FakeShoppingListRepository();
        var householdId = Guid.NewGuid();
        var list = repository.AddList(householdId, "Weekly");
        var access = new FakeHouseholdAccessService();
        var useCase = new UpdateShoppingListUseCase(repository, access);

        var result = await useCase.ExecuteAsync(
            new UpdateShoppingListCommand(Guid.NewGuid(), householdId, list.Id, "Monthly"), CancellationToken.None);

        Assert.Equal("Monthly", result.Name);
        Assert.Equal(1, repository.UpdateCalls);
    }

    [Fact]
    public async Task Delete_ArchivesList()
    {
        var repository = new FakeShoppingListRepository();
        var householdId = Guid.NewGuid();
        var list = repository.AddList(householdId, "Weekly");
        var useCase = new DeleteShoppingListUseCase(repository, new FakeHouseholdAccessService());

        await useCase.ExecuteAsync(
            new DeleteShoppingListCommand(Guid.NewGuid(), householdId, list.Id), CancellationToken.None);

        Assert.NotNull(list.ArchivedAt);
        Assert.Equal(1, repository.UpdateCalls);
    }

    private sealed class FakeHouseholdAccessService(Guid? memberId = null) : IHouseholdAccessService
    {
        public Task EnsureCanAccessHouseholdAsync(Guid householdId, Guid userId, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<Guid> GetRequiredMemberIdAsync(Guid householdId, Guid userId, CancellationToken cancellationToken)
            => Task.FromResult(memberId ?? Guid.NewGuid());
    }

    private sealed class FakeShoppingListRepository : IShoppingListRepository
    {
        public List<ShoppingList> Lists { get; } = [];
        public int UpdateCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public ShoppingList AddList(Guid householdId, string name)
        {
            var now = DateTimeOffset.UtcNow;
            var list = new ShoppingList(Guid.NewGuid(), householdId, name, Guid.NewGuid(), now, now);
            Lists.Add(list);
            return list;
        }

        public Task<IReadOnlyList<ShoppingList>> GetActiveByHouseholdAsync(
            Guid householdId, CancellationToken cancellationToken)
        {
            IReadOnlyList<ShoppingList> lists = Lists
                .Where(list => list.HouseholdId == householdId && list.ArchivedAt is null)
                .OrderBy(list => list.Name)
                .ToList();
            return Task.FromResult(lists);
        }

        public Task<ShoppingList?> GetByIdAsync(Guid shoppingListId, CancellationToken cancellationToken)
            => Task.FromResult(Lists.SingleOrDefault(list => list.Id == shoppingListId));

        public Task AddAsync(ShoppingList shoppingList, CancellationToken cancellationToken)
        {
            Lists.Add(shoppingList);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ShoppingList shoppingList, CancellationToken cancellationToken)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
