using Domu.Api.Features.Users.Application;
using Domu.Api.Features.Users.Application.Ports;
using Domu.Api.Features.Users.Domain;

namespace Domu.Tests.Features.Users.Application;

public sealed class EnsureActorUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsExistingActor_WhenExternalIdentifierAlreadyExists()
    {
        var actor = new Actor(Guid.NewGuid(), "auth0|existing-user");
        var repository = new FakeActorRepository(actor);
        var useCase = new EnsureActorUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new ExternalActorIdentity("auth0|existing-user"),
            CancellationToken.None);

        Assert.Same(actor, result);
        Assert.Equal(0, repository.AddCalls);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesActor_WhenExternalIdentifierDoesNotExist()
    {
        var repository = new FakeActorRepository();
        var useCase = new EnsureActorUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new ExternalActorIdentity("auth0|new-user"),
            CancellationToken.None);

        Assert.Equal("auth0|new-user", result.ExternalIdentifier);
        Assert.Equal(SubscriptionTier.Default, result.SubscriptionTier);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
        Assert.Contains(repository.StoredActors, actor => actor.ExternalIdentifier == "auth0|new-user");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyExternalIdentifier_Throws()
    {
        var repository = new FakeActorRepository();
        var useCase = new EnsureActorUseCase(repository);

        var action = async () => await useCase.ExecuteAsync(
            new ExternalActorIdentity(""),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    private sealed class FakeActorRepository(params Actor[] seededActors) : IActorRepository
    {
        public List<Actor> StoredActors { get; } = seededActors.ToList();
        public int AddCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<Actor?> GetByExternalIdentifierAsync(string externalIdentifier, CancellationToken cancellationToken)
        {
            var actor = StoredActors.SingleOrDefault(existingActor => existingActor.ExternalIdentifier == externalIdentifier);
            return Task.FromResult(actor);
        }

        public Task AddAsync(Actor actor, CancellationToken cancellationToken)
        {
            AddCalls++;
            StoredActors.Add(actor);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
