using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Tests.Features.Households.Domain;

public sealed class HouseholdUnitTest
{
    [Fact]
    public void Constructor_WithEmptyId_Throws()
    {
        var action = () => new Household(Guid.Empty, Guid.NewGuid(), "Home");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Household id cannot be empty.", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyOwnerId_Throws()
    {
        var action = () => new Household(Guid.NewGuid(), Guid.Empty, "Home");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Owner id cannot be empty.", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceName_Throws()
    {
        var action = () => new Household(Guid.NewGuid(), Guid.NewGuid(), " ");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Household name cannot be null or whitespace.", exception.Message);
    }

    [Fact]
    public void Rename_WithTooLongName_Throws()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");
        var tooLongName = new string('A', Household.NameMaxLength + 1);

        var action = () => household.Rename(tooLongName);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains($"Household name cannot be longer than {Household.NameMaxLength} characters.", exception.Message);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var household = new Household(Guid.NewGuid(), Guid.NewGuid(), "Home");

        household.Rename("Apartment");

        Assert.Equal("Apartment", household.Name);
    }
}
