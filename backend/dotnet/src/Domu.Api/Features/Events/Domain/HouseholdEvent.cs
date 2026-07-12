namespace Domu.Api.Features.Events.Domain;

public sealed class HouseholdEvent
{
    public const int ActionMaxLength = 100;
    public const int TargetTypeMaxLength = 64;

    public HouseholdEvent(
        Guid id,
        DateTimeOffset occurredAt,
        Guid actorMemberId,
        string action,
        string targetType,
        Guid? targetId,
        Guid? householdId,
        string metadataJson,
        string? requestId,
        string? clientApp,
        string? clientPlatform,
        string? clientVersion,
        int? clientBuild)
    {
        Id = id == Guid.Empty ? throw new ArgumentException("Event id cannot be empty.", nameof(id)) : id;
        OccurredAt = occurredAt;
        ActorMemberId = actorMemberId == Guid.Empty
            ? throw new ArgumentException("Actor user id cannot be empty.", nameof(actorMemberId))
            : actorMemberId;
        Action = string.IsNullOrWhiteSpace(action)
            ? throw new ArgumentException("Event action cannot be empty.", nameof(action))
            : action;
        TargetType = string.IsNullOrWhiteSpace(targetType)
            ? throw new ArgumentException("Event target type cannot be empty.", nameof(targetType))
            : targetType;
        TargetId = targetId;
        HouseholdId = householdId;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson;
        RequestId = requestId;
        ClientApp = clientApp;
        ClientPlatform = clientPlatform;
        ClientVersion = clientVersion;
        ClientBuild = clientBuild;
    }

    public Guid Id { get; }
    public DateTimeOffset OccurredAt { get; }
    public Guid ActorMemberId { get; }
    public string Action { get; }
    public string TargetType { get; }
    public Guid? TargetId { get; }
    public Guid? HouseholdId { get; }
    public string MetadataJson { get; }
    public string? RequestId { get; }
    public string? ClientApp { get; }
    public string? ClientPlatform { get; }
    public string? ClientVersion { get; }
    public int? ClientBuild { get; }
}
