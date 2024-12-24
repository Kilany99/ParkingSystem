using ParkingSystem.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ParkingSpotId { get; set; }

        public int CarId { get; set; }

        [Required]
        public DateTime EntryTime { get; set; }

        [Required]
        public DateTime? ExitTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Required]
        public string QRCode { get; set; } = string.Empty;

        [Required]
        public SessionStatus Status { get; set; } = SessionStatus.Active;


        public virtual User User { get; set; }
        public virtual Car Car { get; set; }
        public virtual ParkingSpot ParkingSpot { get; set; }
        public virtual Payment Payment { get; set; }

    }
}
