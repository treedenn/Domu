namespace Domu.Api.Features.Activities.Domain;

public sealed class HouseholdActivity
{
    public const int ActionMaxLength = 100;
    public const int TargetTypeMaxLength = 64;

    public HouseholdActivity(
        Guid id,
        DateTimeOffset occurredAt,
        Guid actorId,
        Guid actorMemberId,
        string action,
        string targetType,
        Guid? targetId,
        Guid householdId,
        string metadataJson,
        string? requestId,
        string? clientApp,
        string? clientPlatform,
        string? clientVersion,
        int? clientBuild)
    {
        Id = id == Guid.Empty ? throw new ArgumentException("Activity id cannot be empty.", nameof(id)) : id;
        OccurredAt = occurredAt;
        ActorId = actorId == Guid.Empty
            ? throw new ArgumentException("Actor id cannot be empty.", nameof(actorId))
            : actorId;
        ActorMemberId = actorMemberId == Guid.Empty
            ? throw new ArgumentException("Actor member id cannot be empty.", nameof(actorMemberId))
            : actorMemberId;
        Action = string.IsNullOrWhiteSpace(action)
            ? throw new ArgumentException("Activity action cannot be empty.", nameof(action))
            : action;
        TargetType = string.IsNullOrWhiteSpace(targetType)
            ? throw new ArgumentException("Activity target type cannot be empty.", nameof(targetType))
            : targetType;
        TargetId = targetId;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson;
        RequestId = requestId;
        ClientApp = clientApp;
        ClientPlatform = clientPlatform;
        ClientVersion = clientVersion;
        ClientBuild = clientBuild;
    }

    public Guid Id { get; }
    public DateTimeOffset OccurredAt { get; }
    public Guid ActorId { get; }
    public Guid ActorMemberId { get; }
    public string Action { get; }
    public string TargetType { get; }
    public Guid? TargetId { get; }
    public Guid HouseholdId { get; }
    public string MetadataJson { get; }
    public string? RequestId { get; }
    public string? ClientApp { get; }
    public string? ClientPlatform { get; }
    public string? ClientVersion { get; }
    public int? ClientBuild { get; }
}
