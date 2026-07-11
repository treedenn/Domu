using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Queries;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;
using NSubstitute;

namespace Domu.Tests.Features.ShoppingLists.Application.ShoppingLists;

public sealed class ShoppingListUseCaseTests
{
    [Fact]
    public async Task Create_CreatesShoppingListForHousehold()
    {
        var repository = Substitute.For<IShoppingListRepository>();
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        ShoppingList? createdList = null;

        repository
            .AddAsync(Arg.Do<ShoppingList>(list => createdList = list), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        repository
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var access = CreateAccessService(memberId);
        var useCase = new CreateShoppingListUseCase(repository, access);

        var result = await useCase.ExecuteAsync(
            new CreateShoppingListCommand(userId, householdId, "  Weekly   groceries "), CancellationToken.None);

        Assert.Equal("Weekly groceries", result.Name);
        Assert.Equal(householdId, result.HouseholdId);
        Assert.Equal(memberId, result.CreatedByMemberId);
        Assert.NotNull(createdList);
        Assert.Equal("Weekly groceries", createdList.Name);
        Assert.Equal(householdId, createdList.HouseholdId);
        Assert.Equal(memberId, createdList.CreatedByMemberId);
        await access.Received(1)
            .GetRequiredMemberIdAsync(householdId, userId, Arg.Any<CancellationToken>());
        await repository.Received(1)
            .AddAsync(Arg.Any<ShoppingList>(), Arg.Any<CancellationToken>());
        await repository.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLists_ReturnsRepositoryLists()
    {
        var repository = Substitute.For<IShoppingListRepository>();
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var list = CreateShoppingList(householdId, "Weekly");

        repository
            .GetActiveByHouseholdAsync(householdId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ShoppingList>>([list]));

        var access = CreateAccessService();
        var useCase = new GetShoppingListsUseCase(repository, access);

        var result = await useCase.ExecuteAsync(
            new GetShoppingListsQuery(userId, householdId), CancellationToken.None);

        Assert.Collection(result, listView => Assert.Equal("Weekly", listView.Name));
        await access.Received(1)
            .EnsureCanAccessHouseholdAsync(householdId, userId, Arg.Any<CancellationToken>());
        await repository.Received(1)
            .GetActiveByHouseholdAsync(householdId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_RenamesShoppingList()
    {
        var householdId = Guid.NewGuid();
        var list = CreateShoppingList(householdId, "Weekly");
        var repository = CreateRepositoryReturning(list);
        var access = CreateAccessService();
        var useCase = new UpdateShoppingListUseCase(repository, access);

        var result = await useCase.ExecuteAsync(
            new UpdateShoppingListCommand(Guid.NewGuid(), householdId, list.Id, "Monthly", false), CancellationToken.None);

        Assert.Equal("Monthly", result.Name);
        Assert.Null(result.ArchivedAt);
        await repository.Received(1)
            .UpdateAsync(list, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_ArchivesShoppingList()
    {
        var householdId = Guid.NewGuid();
        var list = CreateShoppingList(householdId, "Weekly");
        var repository = CreateRepositoryReturning(list);
        var access = CreateAccessService();
        var useCase = new UpdateShoppingListUseCase(repository, access);

        var result = await useCase.ExecuteAsync(
            new UpdateShoppingListCommand(Guid.NewGuid(), householdId, list.Id, "Weekly", true), CancellationToken.None);

        Assert.NotNull(result.ArchivedAt);
        await repository.Received(1)
            .UpdateAsync(list, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_ArchivesList()
    {
        var householdId = Guid.NewGuid();
        var list = CreateShoppingList(householdId, "Weekly");
        var repository = CreateRepositoryReturning(list);
        var useCase = new DeleteShoppingListUseCase(repository, CreateAccessService());

        await useCase.ExecuteAsync(
            new DeleteShoppingListCommand(Guid.NewGuid(), householdId, list.Id), CancellationToken.None);

        Assert.NotNull(list.ArchivedAt);
        await repository.Received(1)
            .UpdateAsync(list, Arg.Any<CancellationToken>());
    }

    private static IHouseholdAccessService CreateAccessService(Guid? memberId = null)
    {
        var access = Substitute.For<IHouseholdAccessService>();
        access
            .EnsureCanAccessHouseholdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        access
            .GetRequiredMemberIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(memberId ?? Guid.NewGuid()));

        return access;
    }

    private static IShoppingListRepository CreateRepositoryReturning(ShoppingList list)
    {
        var repository = Substitute.For<IShoppingListRepository>();
        repository
            .GetByIdAsync(list.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));
        repository
            .UpdateAsync(Arg.Any<ShoppingList>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        repository
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return repository;
    }

    private static ShoppingList CreateShoppingList(Guid householdId, string name)
    {
        var now = DateTimeOffset.UtcNow;

        return new ShoppingList(Guid.NewGuid(), householdId, name, Guid.NewGuid(), now, now);
    }
}
