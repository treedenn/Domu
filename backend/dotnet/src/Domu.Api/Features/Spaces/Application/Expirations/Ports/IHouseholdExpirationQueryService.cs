using Domu.Api.Features.Spaces.Application.Expirations.Contracts;

namespace Domu.Api.Features.Spaces.Application.Expirations.Ports;

public interface IHouseholdExpirationQueryService
{
    Task<IReadOnlyList<ExpirationBatchView>> GetAsync(
        Guid householdId,
        DateTimeOffset untilUtc,
        CancellationToken cancellationToken);
}
