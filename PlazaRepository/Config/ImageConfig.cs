using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;

namespace PlazaRepository.Config
{
    public class ImageConfig : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.Property(i => i.Hash).HasMaxLength(128);
            // Relative wwwroot path or a full https URL. Unbounded so Cloudinary URLs fit.
            builder.Property(i => i.Url).HasColumnType("nvarchar(max)");

            // Not unique: existing data already contains duplicate (Hash, Type) rows.
            builder.HasIndex(i => new { i.Hash, i.Type });
        }
    }
}
