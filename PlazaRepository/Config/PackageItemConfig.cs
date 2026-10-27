using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;

namespace PlazaRepository.Config
{
    public class PackageTripConfig : IEntityTypeConfiguration<PackageTrip>
    {
        public void Configure(EntityTypeBuilder<PackageTrip> builder)
        {
            builder.HasOne(x => x.Package)
                .WithMany(p => p.PackageTrips)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Trip)
                .WithMany(t => t.PackageTrips)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PackageId, x.TripId }).IsUnique();
        }
    }

    public class PackageTransportConfig : IEntityTypeConfiguration<PackageTransport>
    {
        public void Configure(EntityTypeBuilder<PackageTransport> builder)
        {
            builder.HasOne(x => x.Package)
                .WithMany(p => p.PackageTransports)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Transport)
                .WithMany(t => t.PackageTransports)
                .HasForeignKey(x => x.TransportId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PackageId, x.TransportId }).IsUnique();
        }
    }

    public class PackageEventConfig : IEntityTypeConfiguration<PackageEvent>
    {
        public void Configure(EntityTypeBuilder<PackageEvent> builder)
        {
            builder.HasOne(x => x.Package)
                .WithMany(p => p.PackageEvents)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Event)
                .WithMany(e => e.PackageEvents)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PackageId, x.EventId }).IsUnique();
        }
    }
}
