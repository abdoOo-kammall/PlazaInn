using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;

namespace PlazaRepository.Config
{
    public class EventConfig : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.Property(e => e.Name).IsRequired().HasMaxLength(150);
            builder.Property(e => e.NameAr).IsRequired().HasMaxLength(150);
            builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
            builder.Property(e => e.DescriptionAr).IsRequired().HasMaxLength(4000);
            builder.Property(e => e.Location).IsRequired().HasMaxLength(250);
            builder.Property(e => e.LocationAr).IsRequired().HasMaxLength(250);
            builder.Property(e => e.TicketPrice).HasPrecision(18, 2);
            builder.Property(e => e.PriceRange).HasMaxLength(200);
            builder.Property(e => e.IsActive).HasDefaultValue(true);

            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.EventDate);
            builder.HasIndex(e => new { e.EventDate, e.EndDate });

            builder.ToTable(e =>
            {
                e.HasCheckConstraint("CK_Events_TicketPrice", "[TicketPrice] > 0");
                e.HasCheckConstraint("CK_Events_TotalTickets", "[TotalTickets] > 0");
                e.HasCheckConstraint("CK_Events_Dates", "[EndDate] IS NULL OR [EndDate] >= [EventDate]");
            });
        }
    }
}
