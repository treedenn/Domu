using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Api.Features.Spaces.Interface.Spaces;

public sealed record CreateSpaceRequest(
    [property: Required]
    [property: MaxLength(Space.NameMaxLength)]
    string Name,
    [property: MaxLength(Space.DescriptionMaxLength)]
    string? Description,
    Guid? ParentId);
