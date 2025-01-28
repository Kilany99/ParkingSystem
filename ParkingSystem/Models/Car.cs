using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{3}\d{4}$", ErrorMessage = "Invalid plate format")]   //validation that plateNumber is in the correct form "XXX1234"
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
