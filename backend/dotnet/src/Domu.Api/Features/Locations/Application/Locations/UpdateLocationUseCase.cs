using Domu.Api.Features.Locations.Application.Locations.Contracts;
using Domu.Api.Features.Locations.Application.Locations.Ports;

namespace Domu.Api.Features.Locations.Application.Locations;

public sealed class UpdateLocationUseCase(ILocationRepository locationRepository) : IUpdateLocationUseCase
{
    public async Task<LocationView> ExecuteAsync(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var location = await locationRepository.GetByIdAsync(command.LocationId, cancellationToken)
                       ?? throw new KeyNotFoundException($"Location '{command.LocationId}' was not found.");

        location.Rename(command.Name);
        location.Describe(command.Description);
        location.MoveTo(command.ParentId);

        await locationRepository.UpdateAsync(location, cancellationToken);
        await locationRepository.SaveChangesAsync(cancellationToken);

        return LocationView.FromDomain(location);
    }
}
