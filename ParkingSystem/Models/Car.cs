using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [StringLength(30)]
        public string Color { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; }

        public virtual ICollection<Reservation> ParkingSessions { get; set; } = new List<Reservation>();
    }
}
