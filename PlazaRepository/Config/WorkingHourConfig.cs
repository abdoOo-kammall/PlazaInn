using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;

namespace PlazaRepository.Config
{
    public class WorkingHourConfig : IEntityTypeConfiguration<WorkingHour>
    {
        public void Configure(EntityTypeBuilder<WorkingHour> builder)
        {
            // Keeps the existing table name so renaming the DbSet doesn't rename the live table.
            builder.ToTable("workingHours");
        }
    }
}
