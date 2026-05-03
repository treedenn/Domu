using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class DeleteSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService)
    : IDeleteSpaceUseCase
{
    public async Task ExecuteAsync(DeleteSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.UserId,
            cancellationToken);

        await spaceRepository.DeleteAsync(command.SpaceId, cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);
    }
}
