using Domu.Api.Features.Events.Domain;

namespace Domu.Api.Features.Events.Infrastructure;

public sealed class UserEventEntity
{
    private UserEventEntity()
    {
    }

    public UserEventEntity(UserEvent userEvent)
    {
        Id = userEvent.Id;
        OccurredAt = userEvent.OccurredAt;
        ActorUserId = userEvent.ActorUserId;
        HouseholdId = userEvent.HouseholdId;
        Action = userEvent.Action;
        TargetType = userEvent.TargetType;
        TargetId = userEvent.TargetId;
        MetadataJson = userEvent.MetadataJson;
        RequestId = userEvent.RequestId;
        ClientApp = userEvent.ClientApp;
        ClientPlatform = userEvent.ClientPlatform;
        ClientVersion = userEvent.ClientVersion;
        ClientBuild = userEvent.ClientBuild;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public Guid ActorUserId { get; private set; }
    public Guid? HouseholdId { get; private set; }
    public string Action { get; private set; } = null!;
    public string TargetType { get; private set; } = null!;
    public Guid? TargetId { get; private set; }
    public string MetadataJson { get; private set; } = "{}";
    public string? RequestId { get; private set; }
    public string? ClientApp { get; private set; }
    public string? ClientPlatform { get; private set; }
    public string? ClientVersion { get; private set; }
    public int? ClientBuild { get; private set; }

    public UserEvent ToDomain()
    {
        return new UserEvent(
            Id,
            OccurredAt,
            ActorUserId,
            Action,
            TargetType,
            TargetId,
            HouseholdId,
            MetadataJson,
            RequestId,
            ClientApp,
            ClientPlatform,
            ClientVersion,
            ClientBuild);
    }
}
