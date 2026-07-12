using Domu.Api.Features.Events.Domain;

namespace Domu.Api.Features.Events.Infrastructure;

public sealed class HouseholdEventEntity
{
    private HouseholdEventEntity()
    {
    }

    public HouseholdEventEntity(HouseholdEvent userEvent)
    {
        Id = userEvent.Id;
        OccurredAt = userEvent.OccurredAt;
        ActorMemberId = userEvent.ActorMemberId;
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

    public Guid Id { get; }
    public DateTimeOffset OccurredAt { get; }
    public Guid ActorMemberId { get; }
    public Guid? HouseholdId { get; }
    public string Action { get; } = null!;
    public string TargetType { get; } = null!;
    public Guid? TargetId { get; }
    public string MetadataJson { get; } = "{}";
    public string? RequestId { get; }
    public string? ClientApp { get; }
    public string? ClientPlatform { get; }
    public string? ClientVersion { get; }
    public int? ClientBuild { get; }

    public HouseholdEvent ToDomain()
    {
        return new HouseholdEvent(
            Id,
            OccurredAt,
            ActorMemberId,
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