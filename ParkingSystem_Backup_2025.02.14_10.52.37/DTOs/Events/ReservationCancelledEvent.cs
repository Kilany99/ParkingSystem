using ParkingSystem.Models;

namespace ParkingSystem.DTOs.Events
{
    public class ReservationCancelledEvent
    {
        public int ReservationId { get; set; }
        public string ParkingZoneName { get; set; }
        public string Email { get; set; }
        public DateTime CancelledAt { get; set; }
    }
}
