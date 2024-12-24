using Microsoft.EntityFrameworkCore;
using ParkingSystem.Models;


namespace ParkingSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        { 
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-P944USQ\SQLEXPRESS01;Initial Catalog=ParkingSystemDB;TrustServerCertificate=True;Integrated Security=True;");
        }
    }
}
