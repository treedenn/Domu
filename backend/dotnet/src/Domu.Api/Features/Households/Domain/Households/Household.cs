namespace Domu.Api.Features.Households.Domain.Households;

public sealed class Household
{
    public const int NameMaxLength = 100;

    private string _name = null!;

    public Household(Guid id, Guid ownerId, string name)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(id))
            : id;
        OwnerId = ownerId == Guid.Empty
            ? throw new ArgumentException("Owner id cannot be empty.", nameof(ownerId))
            : ownerId;
        Rename(name);
    }

    public Guid Id { get; }
    public Guid OwnerId { get; }

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Household name cannot be null or whitespace.", nameof(name));
        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Household name cannot be longer than {NameMaxLength} characters.", nameof(name));

        Name = name;
    }
}
