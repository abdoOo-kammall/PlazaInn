using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PlazaRepository.Config
{
    public class bookingConfig : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder
                .HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(b => b.Hotel)
                .WithMany(h => h.Bookings)
                .HasForeignKey(b => b.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(b => b.Trip)
                .WithMany()
                .HasForeignKey(b => b.TripId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(b => b.Transport)
                .WithMany()
                .HasForeignKey(b => b.TransportId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(b => b.Package)
                .WithMany()
                .HasForeignKey(b => b.PackageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(b => b.CustomerPhone).HasMaxLength(32);
            builder.Property(b => b.MerchantOrderId).HasMaxLength(100);
            builder.Property(b => b.CustomerEmail).HasMaxLength(256);
            builder.Property(b => b.PaymentReference).HasMaxLength(64);
            builder.Property(b => b.TotalPrice).HasPrecision(18, 2);
            builder.Property(b => b.BookingType).HasDefaultValue(BookingType.Hotel);
            builder.Property(b => b.Quantity).HasDefaultValue(1);

            builder.HasIndex(b => b.PaymentReference);
            builder.HasIndex(b => b.CustomerEmail);
            builder.HasIndex(b => new { b.HotelId, b.CheckIn, b.CheckOut });
            builder.HasIndex(b => new { b.Status, b.CreatedAt });
            builder.HasIndex(b => new { b.BookingType, b.TripId, b.ServiceDate });
            builder.HasIndex(b => new { b.BookingType, b.TransportId, b.ServiceDate });
            builder.HasIndex(b => new { b.BookingType, b.EventId });
            builder.HasIndex(b => new { b.BookingType, b.PackageId });

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Bookings_Quantity", "[Quantity] > 0");
                t.HasCheckConstraint(
                    "CK_Bookings_Target",
                    "([BookingType] = 0 AND [HotelId] IS NOT NULL AND [RoomId] IS NOT NULL AND [TripId] IS NULL AND [TransportId] IS NULL AND [EventId] IS NULL AND [PackageId] IS NULL) OR " +
                    "([BookingType] = 1 AND [TripId] IS NOT NULL AND [HotelId] IS NULL AND [RoomId] IS NULL AND [TransportId] IS NULL AND [EventId] IS NULL AND [PackageId] IS NULL AND [ServiceDate] IS NOT NULL) OR " +
                    "([BookingType] = 2 AND [TransportId] IS NOT NULL AND [HotelId] IS NULL AND [RoomId] IS NULL AND [TripId] IS NULL AND [EventId] IS NULL AND [PackageId] IS NULL AND [ServiceDate] IS NOT NULL) OR " +
                    "([BookingType] = 3 AND [EventId] IS NOT NULL AND [HotelId] IS NULL AND [RoomId] IS NULL AND [TripId] IS NULL AND [TransportId] IS NULL AND [PackageId] IS NULL) OR " +
                    "([BookingType] = 4 AND [PackageId] IS NOT NULL AND [TripId] IS NULL AND [TransportId] IS NULL AND [EventId] IS NULL)");
            });
        }
    }
}
