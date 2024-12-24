using ParkingSystem.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ParkingSystem.Models
{
    public class ParkingSpot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string SpotNumber { get; set; } = string.Empty;
       
        [Required]
        public SpotStatus Status { get; set; } = SpotStatus.Available;

        public SpotType Type { get; set; } = SpotType.Standard;  // Type of parking spot


        public int Floor { get; set; }

        public int? ReservationId { get; set; }


        public int ParkingZoneId { get; set; }

        // Navigation property
        public virtual ParkingZone ParkingZone { get; set; }  
        public virtual Reservation? CurrentReservation { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    }
}
