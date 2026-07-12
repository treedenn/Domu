using Domu.Api.Features.Activities.Domain;

namespace Domu.Api.Features.Activities.Infrastructure;

public sealed class HouseholdActivityEntity
{
    private HouseholdActivityEntity()
    {
    }

    public HouseholdActivityEntity(HouseholdActivity householdActivity)
    {
        Id = householdActivity.Id;
        OccurredAt = householdActivity.OccurredAt;
        ActorId = householdActivity.ActorId;
        ActorMemberId = householdActivity.ActorMemberId;
        HouseholdId = householdActivity.HouseholdId;
        Action = householdActivity.Action;
        TargetType = householdActivity.TargetType;
        TargetId = householdActivity.TargetId;
        MetadataJson = householdActivity.MetadataJson;
        RequestId = householdActivity.RequestId;
        ClientApp = householdActivity.ClientApp;
        ClientPlatform = householdActivity.ClientPlatform;
        ClientVersion = householdActivity.ClientVersion;
        ClientBuild = householdActivity.ClientBuild;
    }

    public Guid Id { get; }
    public DateTimeOffset OccurredAt { get; }
    public Guid ActorId { get; }
    public Guid ActorMemberId { get; }
    public Guid HouseholdId { get; }
    public string Action { get; } = null!;
    public string TargetType { get; } = null!;
    public Guid? TargetId { get; }
    public string MetadataJson { get; } = "{}";
    public string? RequestId { get; }
    public string? ClientApp { get; }
    public string? ClientPlatform { get; }
    public string? ClientVersion { get; }
    public int? ClientBuild { get; }

    public HouseholdActivity ToDomain()
    {
        return new HouseholdActivity(
            Id,
            OccurredAt,
            ActorId,
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
