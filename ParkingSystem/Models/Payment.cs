using ParkingSystem.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Reservation")]
        public int ReservationId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        public PaymentMethod Method { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation property
        public virtual Reservation Reservation { get; set; }
    }
}
