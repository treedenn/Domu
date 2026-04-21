using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Api.Features.Households.Infrastructure.Households;

public sealed class HouseholdEntity
{
    private HouseholdEntity()
    {
    }

    public HouseholdEntity(Guid id, Guid ownerId, string name)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(id))
            : id;
        OwnerId = ownerId == Guid.Empty
            ? throw new ArgumentException("Owner id cannot be empty.", nameof(ownerId))
            : ownerId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Household name cannot be empty.", nameof(name))
            : name;
    }

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = null!;

    public Household ToDomain()
    {
        return new Household(Id, OwnerId, Name);
    }

    public static HouseholdEntity FromDomain(Household household)
    {
        ArgumentNullException.ThrowIfNull(household);

        return new HouseholdEntity(household.Id, household.OwnerId, household.Name);
    }

    public void UpdateFromDomain(Household household)
    {
        ArgumentNullException.ThrowIfNull(household);
        if (household.Id != Id)
            throw new ArgumentException("Cannot update household entity from a different household.", nameof(household));

        OwnerId = household.OwnerId;
        Name = household.Name;
    }
}
