namespace ParkingSystem.Enums
{
    public enum SessionStatus
    {
        Reserved,   // Initial state when reservation is created
        Active,     // When parking has started
        Completed,  // When parking has ended
        Cancelled   // When reservation is cancelled
    }
}
