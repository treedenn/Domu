using Domu.Api.Features.Users.Application;
using Domu.Api.Features.Users.Application.Ports;
using Domu.Api.Features.Users.Domain;

namespace Domu.Tests.Features.Users.Application;

public sealed class EnsureUserUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsExistingActor_WhenExternalIdentifierAlreadyExists()
    {
        var user = new AuthenticatedUser(Guid.NewGuid());
        var repository = new FakeUserRepository((user, "auth0|existing-user"));
        var useCase = new EnsureUserUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new UserAuthIdentity("auth0|existing-user"),
            CancellationToken.None);

        Assert.Same(user, result);
        Assert.Equal(0, repository.AddCalls);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesActor_WhenExternalIdentifierDoesNotExist()
    {
        var repository = new FakeUserRepository();
        var useCase = new EnsureUserUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new UserAuthIdentity("auth0|new-user"),
            CancellationToken.None);

        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
        Assert.Contains(repository.StoredUsers, entry => entry.User == result && entry.ExternalIdentifier == "auth0|new-user");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyExternalIdentifier_Throws()
    {
        var repository = new FakeUserRepository();
        var useCase = new EnsureUserUseCase(repository);

        var action = async () => await useCase.ExecuteAsync(
            new UserAuthIdentity(""),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    private sealed class FakeUserRepository(params (AuthenticatedUser User, string ExternalIdentifier)[] seededUsers) : IUserRepository
    {
        public List<(AuthenticatedUser User, string ExternalIdentifier)> StoredUsers { get; } = seededUsers.ToList();
        public int AddCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<AuthenticatedUser?> GetByAuthIdentityAsync(string externalIdentifier, CancellationToken cancellationToken)
        {
            AuthenticatedUser? user = StoredUsers
                .Where(existingUser => existingUser.ExternalIdentifier == externalIdentifier)
                .Select(existingUser => existingUser.User)
                .SingleOrDefault();
            return Task.FromResult(user);
        }

        public Task AddAsync(AuthenticatedUser authenticatedUser, string externalIdentifier, CancellationToken cancellationToken)
        {
            AddCalls++;
            StoredUsers.Add((authenticatedUser, externalIdentifier));
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
