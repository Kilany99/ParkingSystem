using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class ParkingZone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int TotalFloors { get; set; }

        [Required]
        public int SpotsPerFloor { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public string? Description { get; set; }

        [Required]
        public bool IsFull { get; set; } = false;


        // Navigation properties
        public virtual ICollection<ParkingSpot> ParkingSpots { get; set; } = new List<ParkingSpot>();
        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    }
}
