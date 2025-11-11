using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlazaCore.Entites;

namespace PlazaRepository
{
    public class PlazaDbContext : IdentityDbContext<ApplicationUser>
    {
        public PlazaDbContext(DbContextOptions<PlazaDbContext> options) : base(options) 
        {
                

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Image> Images { get; set; }
public   DbSet<Partners> Partners { get; set; }

    }
}
