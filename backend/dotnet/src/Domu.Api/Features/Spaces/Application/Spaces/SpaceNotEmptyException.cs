namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class SpaceNotEmptyException : InvalidOperationException
{
    public const string Detail = "A space with child spaces or items cannot be deleted.";

    public SpaceNotEmptyException() : base(Detail)
    {
    }
}
