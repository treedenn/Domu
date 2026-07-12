using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record GetSpacesPageQuery(
    DomuActor Actor,
    Guid HouseholdId,
    Guid? ParentId,
    int PageNumber = 1,
    int PageSize = 20,
    SpaceItemsProjection Items = SpaceItemsProjection.None,
    SpaceChildrenProjection Children = SpaceChildrenProjection.None);
