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
        public DbSet<Partners> Partners { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<HomeSlider> HomeSlider { get; set; }
        public DbSet<AboutUs> AboutUs { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<WorkingHour> WorkingHours { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Transport> Transports { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageTrip> PackageTrips { get; set; }
        public DbSet<PackageTransport> PackageTransports { get; set; }
        public DbSet<PackageEvent> PackageEvents { get; set; }
  
    }
}
