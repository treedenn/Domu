using Domu.Api.Features.Locations.Application.Locations.Contracts;

namespace Domu.Api.Features.Locations.Application.Locations.Ports;

public interface ILocationQueryService
{
    Task<LocationPage> GetPageAsync(GetLocationsPageQuery query, CancellationToken cancellationToken);
}
