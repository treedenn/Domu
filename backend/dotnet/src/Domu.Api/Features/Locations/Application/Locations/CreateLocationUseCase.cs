using Domu.Api.Features.Locations.Application.Locations.Contracts;
using Domu.Api.Features.Locations.Application.Locations.Ports;
using Domu.Api.Features.Locations.Domain.Locations;

namespace Domu.Api.Features.Locations.Application.Locations;

public sealed class CreateLocationUseCase(ILocationRepository locationRepository) : ICreateLocationUseCase
{
    public async Task<LocationView> ExecuteAsync(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var location = new Location(Guid.CreateVersion7(), command.Name, command.OwnerId);
        location.Describe(command.Description);
        location.MoveTo(command.ParentId);

        await locationRepository.AddAsync(location, cancellationToken);
        await locationRepository.SaveChangesAsync(cancellationToken);

        return LocationView.FromDomain(location);
    }
}
