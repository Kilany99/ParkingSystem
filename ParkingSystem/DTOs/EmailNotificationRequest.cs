namespace ParkingSystem.DTOs
{
    public class EmailNotificationRequest
    {
        public required string Message { get; set; }
        public required string Email { get; set; }
        public required string QrCode { get; set; }
    }
}
