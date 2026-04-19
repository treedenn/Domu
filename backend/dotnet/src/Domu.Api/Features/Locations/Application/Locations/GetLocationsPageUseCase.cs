using Domu.Api.Features.Locations.Application.Locations.Contracts;
using Domu.Api.Features.Locations.Application.Locations.Ports;

namespace Domu.Api.Features.Locations.Application.Locations;

public sealed class GetLocationsPageUseCase(ILocationQueryService locationQueryService) : IGetLocationsPageUseCase
{
    public Task<LocationPage> ExecuteAsync(GetLocationsPageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.PageNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(query.PageNumber), "Page number must be >= 1.");
        if (query.PageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(query.PageSize), "Page size must be >= 1.");

        return locationQueryService.GetPageAsync(query, cancellationToken);
    }
}
