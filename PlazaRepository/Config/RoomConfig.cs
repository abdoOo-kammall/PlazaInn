using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlazaCore.Entites;

namespace PlazaRepository.Config
{
    public class RoomConfig : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder
               .Property(r => r.Features)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                   v => string.IsNullOrEmpty(v)
                       ? new Dictionary<string, bool>()
                       : JsonSerializer.Deserialize<Dictionary<string, bool>>(v, (JsonSerializerOptions?)null)
               );

            
            builder.Property(r => r.Features).IsRequired(false);
        }
    }
}
