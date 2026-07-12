using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Api.Features.Spaces.Interface.Spaces;

public sealed record UpdateSpaceRequest(
    [Required]
    [MaxLength(Space.NameMaxLength)]
    string Name,
    [MaxLength(Space.DescriptionMaxLength)]
    string? Description);