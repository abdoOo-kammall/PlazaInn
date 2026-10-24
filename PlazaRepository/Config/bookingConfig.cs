using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;
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
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(b => b.Hotel)
                .WithMany(h => h.Bookings)
                .HasForeignKey(b => b.HotelId)
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

            builder.HasIndex(b => b.PaymentReference);
            builder.HasIndex(b => b.CustomerEmail);
            builder.HasIndex(b => new { b.HotelId, b.CheckIn, b.CheckOut });
            builder.HasIndex(b => new { b.Status, b.CreatedAt });
        }
    }
}
