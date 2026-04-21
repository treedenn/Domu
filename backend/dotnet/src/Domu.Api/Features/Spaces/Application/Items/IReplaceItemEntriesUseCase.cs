using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public interface IReplaceItemEntriesUseCase
{
    Task<ItemView> ExecuteAsync(ReplaceItemEntriesCommand command, CancellationToken cancellationToken);
}
