using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Tests.Features.ShoppingLists.Application.ShoppingLists;

public sealed class GetOrCreateDefaultShoppingListUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesDefaultListWhenNoneExists()
    {
        var repository = new FakeShoppingListRepository();
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var useCase = new GetOrCreateDefaultShoppingListUseCase(repository, new FakeHouseholdAccessService());

        var result = await useCase.ExecuteAsync(
            new GetOrCreateDefaultShoppingListQuery(userId, householdId),
            CancellationToken.None);

        Assert.Equal(householdId, result.HouseholdId);
        Assert.True(result.IsDefault);
        Assert.Equal(userId, result.CreatedByUserId);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
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

    private sealed class FakeShoppingListRepository : IShoppingListRepository
    {
        public List<ShoppingList> Lists { get; } = [];
        public int AddCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

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
            AddCalls++;
            Lists.Add(shoppingList);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
