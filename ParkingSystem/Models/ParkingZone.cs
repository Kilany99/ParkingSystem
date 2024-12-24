using System.ComponentModel.DataAnnotations;

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

        public string? Description { get; set; }

        [Required]
        public bool IsFull { get; set; } = false;


        // Navigation property
        public virtual ICollection<ParkingSpot> ParkingSpots { get; set; } = new List<ParkingSpot>();

    }
}
