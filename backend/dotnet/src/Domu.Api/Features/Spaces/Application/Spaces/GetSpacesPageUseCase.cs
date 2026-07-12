using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class GetSpacesPageUseCase(
    ISpaceQueryService spaceQueryService,
    ISpaceAccessService spaceAccessService)
    : IGetSpacesPageUseCase
{
    public async Task<SpacePage> ExecuteAsync(GetSpacesPageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.PageNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(query.PageNumber), "Page number must be greater than zero.");
        if (query.PageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(query.PageSize), "Page size must be greater than zero.");

        await spaceAccessService.EnsureCanAccessSpaceTargetAsync(
            query.HouseholdId,
            query.ParentId,
            query.Actor,
            cancellationToken);

        return await spaceQueryService.GetPageAsync(query, cancellationToken);
    }
}