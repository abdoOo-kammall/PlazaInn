using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;

namespace PlazaRepository.Config
{
    public class TripConfig : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
            builder.Property(t => t.NameAr).IsRequired().HasMaxLength(150);
            builder.Property(t => t.Description).IsRequired().HasMaxLength(4000);
            builder.Property(t => t.DescriptionAr).IsRequired().HasMaxLength(4000);
            builder.Property(t => t.City).IsRequired().HasMaxLength(100);
            builder.Property(t => t.CityAr).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Itinerary).IsRequired().HasMaxLength(8000);
            builder.Property(t => t.ItineraryAr).IsRequired().HasMaxLength(8000);
            builder.Property(t => t.PricePerPerson).HasPrecision(18, 2);
            builder.Property(t => t.PricePerChild).HasPrecision(18, 2);
            builder.Property(t => t.PriceRange).HasMaxLength(200);
            builder.Property(t => t.IsActive).HasDefaultValue(true);

            builder.HasIndex(t => t.IsActive);
            builder.HasIndex(t => t.City);
            builder.HasIndex(t => new { t.StartDate, t.EndDate });

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Trips_PricePerPerson", "[PricePerPerson] > 0");
                t.HasCheckConstraint("CK_Trips_PricePerChild", "[PricePerChild] > 0");
                t.HasCheckConstraint("CK_Trips_MaxCapacity", "[MaxCapacity] > 0");
                t.HasCheckConstraint("CK_Trips_DurationDays", "[DurationDays] >= 0");
                t.HasCheckConstraint("CK_Trips_DurationHours", "[DurationHours] >= 0");
                t.HasCheckConstraint("CK_Trips_Duration", "[DurationDays] > 0 OR [DurationHours] > 0");
                t.HasCheckConstraint("CK_Trips_Dates", "[EndDate] IS NULL OR ([StartDate] IS NOT NULL AND [EndDate] >= [StartDate])");
            });
        }
    }
}
