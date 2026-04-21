using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class CreateSpaceUseCase(ISpaceRepository spaceRepository) : ICreateSpaceUseCase
{
    public async Task<SpaceView> ExecuteAsync(CreateSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var space = new Space(Guid.CreateVersion7(), command.Name, command.HouseholdId);
        space.Describe(command.Description);
        space.MoveTo(command.ParentId);

        await spaceRepository.AddAsync(space, cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);

        return SpaceView.FromDomain(space);
    }
}
