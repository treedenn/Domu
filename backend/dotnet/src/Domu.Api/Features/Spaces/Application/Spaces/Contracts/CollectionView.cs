namespace Domu.Api.Features.Spaces.Application.Spaces.Contracts;

public sealed record CollectionView<T>(int Count, IReadOnlyList<T>? Data);
