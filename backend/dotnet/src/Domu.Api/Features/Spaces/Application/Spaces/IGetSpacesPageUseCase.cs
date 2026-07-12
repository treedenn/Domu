using Domu.Api.Features.Spaces.Application.Spaces.Contracts;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public interface IGetSpacesPageUseCase
{
    Task<SpacePage> ExecuteAsync(GetSpacesPageQuery query, CancellationToken cancellationToken);
}