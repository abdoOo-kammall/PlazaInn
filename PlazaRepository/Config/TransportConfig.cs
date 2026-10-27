using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;

namespace PlazaRepository.Config
{
    public class TransportConfig : IEntityTypeConfiguration<Transport>
    {
        public void Configure(EntityTypeBuilder<Transport> builder)
        {
            builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
            builder.Property(t => t.NameAr).IsRequired().HasMaxLength(150);
            builder.Property(t => t.Description).IsRequired().HasMaxLength(4000);
            builder.Property(t => t.DescriptionAr).IsRequired().HasMaxLength(4000);
            builder.Property(t => t.VehicleType).IsRequired().HasMaxLength(100);
            builder.Property(t => t.VehicleTypeAr).IsRequired().HasMaxLength(100);
            builder.Property(t => t.PickupLocation).IsRequired().HasMaxLength(250);
            builder.Property(t => t.PickupLocationAr).IsRequired().HasMaxLength(250);
            builder.Property(t => t.DropoffLocation).IsRequired().HasMaxLength(250);
            builder.Property(t => t.DropoffLocationAr).IsRequired().HasMaxLength(250);
            builder.Property(t => t.PricePerTrip).HasPrecision(18, 2);
            builder.Property(t => t.IsActive).HasDefaultValue(true);

            builder.HasIndex(t => t.IsActive);
            builder.HasIndex(t => t.VehicleType);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Transports_PricePerTrip", "[PricePerTrip] > 0");
                t.HasCheckConstraint("CK_Transports_MaxPassengers", "[MaxPassengers] > 0");
                t.HasCheckConstraint("CK_Transports_MaxLuggage", "[MaxLuggage] >= 0");
                t.HasCheckConstraint("CK_Transports_AvailableVehicles", "[AvailableVehicles] > 0");
            });
        }
    }
}
