namespace ParkingSystem.DTOs
{
    public class ReservationCreatedEvent
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string QRCode { get; set; }
    }

}
