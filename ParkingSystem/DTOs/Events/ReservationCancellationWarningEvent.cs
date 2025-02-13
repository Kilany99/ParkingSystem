namespace ParkingSystem.DTOs.Events
{
    public class ReservationCancellationWarningEvent
    {
        public string Email { get; set; }
        public DateTime CancellationTime { get; set; }
    }
}
