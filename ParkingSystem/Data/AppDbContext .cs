using Microsoft.EntityFrameworkCore;
using ParkingSystem.Models;


namespace ParkingSystem.Data
{
    public class AppDbContext : DbContext
    {
       
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<ParkingZone> ParkingZones { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=host.docker.internal,1433;Database=ParkingSystemDB;User Id=parking_user;Password=1999King;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Car configurations
            modelBuilder.Entity<Car>()
                .HasIndex(c => c.PlateNumber)
                .IsUnique();

            // ParkingSpot configurations
            modelBuilder.Entity<ParkingSpot>()
                .HasOne(ps => ps.ParkingZone)
                .WithMany(pz => pz.ParkingSpots)
                .HasForeignKey(ps => ps.ParkingZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation configurations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Car)
                .WithMany(c => c.ParkingSessions)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.ParkingSpot)
                .WithMany(ps => ps.Reservations)
                .HasForeignKey(r => r.ParkingSpotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment configurations
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithOne(r => r.Payment)
                .HasForeignKey<Payment>(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
