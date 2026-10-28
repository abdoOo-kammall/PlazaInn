using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;

namespace PlazaRepository.Config
{
    public class PackageConfig : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
            builder.Property(p => p.NameAr).IsRequired().HasMaxLength(150);
            builder.Property(p => p.Description).IsRequired().HasMaxLength(4000);
            builder.Property(p => p.DescriptionAr).IsRequired().HasMaxLength(4000);
            builder.Property(p => p.PackagePrice).HasPrecision(18, 2);
            builder.Property(p => p.PriceRange).HasMaxLength(200);
            builder.Property(p => p.IsActive).HasDefaultValue(true);

            builder.HasOne(p => p.Hotel)
                .WithMany()
                .HasForeignKey(p => p.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Room)
                .WithMany()
                .HasForeignKey(p => p.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => new { p.StartDate, p.EndDate });
            builder.HasIndex(p => p.HotelId);
            builder.HasIndex(p => p.RoomId);

            builder.ToTable(p =>
            {
                p.HasCheckConstraint("CK_Packages_PackagePrice", "[PackagePrice] > 0");
                p.HasCheckConstraint("CK_Packages_MaxPackagesAvailable", "[MaxPackagesAvailable] > 0");
                p.HasCheckConstraint("CK_Packages_DurationDays", "[DurationDays] > 0");
                p.HasCheckConstraint("CK_Packages_Dates", "[EndDate] IS NULL OR ([StartDate] IS NOT NULL AND [EndDate] >= [StartDate])");
                p.HasCheckConstraint(
                    "CK_Packages_HotelStayComplete",
                    "([HotelId] IS NULL AND [RoomId] IS NULL AND [NightsCount] IS NULL) OR ([HotelId] IS NOT NULL AND [RoomId] IS NOT NULL AND [NightsCount] IS NOT NULL AND [NightsCount] > 0)");
            });
        }
    }
}
