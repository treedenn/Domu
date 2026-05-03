using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domu.Api.Features.Households.Infrastructure.Households;

public sealed class HouseholdConfiguration : IEntityTypeConfiguration<HouseholdEntity>
{
    public void Configure(EntityTypeBuilder<HouseholdEntity> builder)
    {
        builder.ToTable("households");

        builder.HasKey(household => household.Id);

        builder.Property(household => household.Id)
            .ValueGeneratedNever();

        builder.Property(household => household.OwnerId)
            .IsRequired();

        builder.Property(household => household.Name)
            .HasMaxLength(Domain.Households.Household.NameMaxLength)
            .IsRequired();

        builder.Property(household => household.SubscriptionPlan)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(household => household.SubscriptionStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(household => new { household.OwnerId, household.Name });
    }
}
