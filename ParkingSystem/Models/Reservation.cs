using ParkingSystem.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EntryTime { get; set; }

        public DateTime? ExitTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        public bool IsPaid { get; set; } = false;

        [Required]
        public string QRCode { get; set; } = string.Empty;

        [Required]
        public SessionStatus Status { get; set; } = SessionStatus.Reserved;


        public virtual User User { get; set; }
        public virtual Car Car { get; set; }
        [JsonIgnore]
        public virtual ParkingSpot ParkingSpot { get; set; }
        public virtual Payment Payment { get; set; }

    }
}
