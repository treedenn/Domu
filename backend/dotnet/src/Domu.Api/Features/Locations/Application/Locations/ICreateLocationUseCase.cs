using Domu.Api.Features.Locations.Application.Locations.Contracts;

namespace Domu.Api.Features.Locations.Application.Locations;

public interface ICreateLocationUseCase
{
    Task<LocationView> ExecuteAsync(CreateLocationCommand command, CancellationToken cancellationToken);
}
